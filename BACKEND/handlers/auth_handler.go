package handlers

import (
	"errors"
	"net/http"
	"net/mail"
	"os"
	"regexp"
	"time"

	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"golang.org/x/crypto/bcrypt"

	"github.com/oussema-fatnassi/WarOfTanks/backend/models"
	"github.com/oussema-fatnassi/WarOfTanks/backend/services"
)

var usernameRegex = regexp.MustCompile(`^[a-zA-Z0-9_]{3,20}$`)

type AuthHandler struct {
	players *mongo.Collection
	jwtSvc  *services.JWTService
}

func NewAuthHandler(db *mongo.Database, jwtSvc *services.JWTService) *AuthHandler {
	return &AuthHandler{
		players: db.Collection("players"),
		jwtSvc:  jwtSvc,
	}
}

// RegisterRequest is the expected body for POST /auth/register.
type RegisterRequest struct {
	Username string `json:"username" binding:"required" example:"tank_commander"`
	Email    string `json:"email" binding:"required" example:"tank@example.com"`
	Password string `json:"password" binding:"required" example:"strong-password"`
}

// LoginRequest is the expected body for POST /auth/login.
type LoginRequest struct {
	Username string `json:"username" binding:"required" example:"tank_commander"`
	Password string `json:"password" binding:"required" example:"strong-password"`
}

// Register godoc
// @Summary Register a player
// @Description Creates a new player account. The password is hashed before storage and is never returned.
// @Tags auth
// @Accept json
// @Produce json
// @Param request body RegisterRequest true "Registration payload"
// @Success 201 {object} RegisterResponse
// @Failure 400 {object} ErrorResponse
// @Failure 409 {object} ErrorResponse
// @Failure 500 {object} ErrorResponse
// @Router /auth/register [post]
func (h *AuthHandler) Register(c *gin.Context) {
	var req RegisterRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "username, email and password are required"})
		return
	}

	// Validate username
	if !usernameRegex.MatchString(req.Username) {
		c.JSON(http.StatusBadRequest, gin.H{"error": "username must be 3–20 characters (letters, digits, underscore only)"})
		return
	}

	// Validate email
	if _, err := mail.ParseAddress(req.Email); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "email must be valid"})
		return
	}

	// Validate password
	if len(req.Password) < 8 {
		c.JSON(http.StatusBadRequest, gin.H{"error": "password must be at least 8 characters"})
		return
	}

	ctx := c.Request.Context()

	// Check uniqueness
	var existing models.Player
	err := h.players.FindOne(ctx, bson.D{{Key: "username", Value: req.Username}}).Decode(&existing)
	if err == nil {
		c.JSON(http.StatusConflict, gin.H{"error": "username already taken"})
		return
	}
	if !errors.Is(err, mongo.ErrNoDocuments) {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "database error"})
		return
	}

	// Hash password
	hash, err := bcrypt.GenerateFromPassword([]byte(req.Password), bcrypt.DefaultCost)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not hash password"})
		return
	}

	now := time.Now().UTC()
	player := models.Player{
		ID:           bson.NewObjectID(),
		Username:     req.Username,
		Email:        req.Email,
		PasswordHash: string(hash),
		Stats:        models.PlayerStats{},
		CreatedAt:    now,
		UpdatedAt:    now,
	}

	if _, err := h.players.InsertOne(ctx, player); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not create player"})
		return
	}

	c.JSON(http.StatusCreated, gin.H{
		"id":       player.ID.Hex(),
		"username": player.Username,
		"email":    player.Email,
	})
}

// Login godoc
// @Summary Login
// @Description Authenticates a player, returns a one-hour access token, and sets the seven-day refresh token as an HttpOnly cookie.
// @Tags auth
// @Accept json
// @Produce json
// @Param request body LoginRequest true "Login payload"
// @Success 200 {object} LoginResponse
// @Failure 400 {object} ErrorResponse
// @Failure 401 {object} ErrorResponse
// @Failure 500 {object} ErrorResponse
// @Router /auth/login [post]
func (h *AuthHandler) Login(c *gin.Context) {
	var req LoginRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "username and password are required"})
		return
	}

	ctx := c.Request.Context()

	var player models.Player
	err := h.players.FindOne(ctx, bson.D{{Key: "username", Value: req.Username}}).Decode(&player)
	if err != nil {
		// Don't reveal whether the username exists
		c.JSON(http.StatusUnauthorized, gin.H{"error": "invalid credentials"})
		return
	}

	if err := bcrypt.CompareHashAndPassword([]byte(player.PasswordHash), []byte(req.Password)); err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "invalid credentials"})
		return
	}

	accessToken, err := h.jwtSvc.GenerateAccessToken(player.ID.Hex(), player.Username)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not generate token"})
		return
	}

	refreshToken, err := h.jwtSvc.GenerateRefreshToken(player.ID.Hex(), player.Username)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not generate token"})
		return
	}

	_, _ = h.players.UpdateOne(ctx,
		bson.D{{Key: "_id", Value: player.ID}},
		bson.D{{Key: "$set", Value: bson.D{
			{Key: "refreshToken", Value: refreshToken},
			{Key: "updatedAt", Value: time.Now().UTC()},
		}}},
	)

	h.setRefreshCookie(c, refreshToken, int((7 * 24 * time.Hour).Seconds()))

	c.JSON(http.StatusOK, gin.H{
		"accessToken": accessToken,
		"player":      player,
	})
}

// Refresh godoc
// @Summary Refresh access token
// @Description Reads the HttpOnly refresh_token cookie and returns a new one-hour access token. No refresh token is returned in JSON.
// @Tags auth
// @Produce json
// @Success 200 {object} RefreshResponse
// @Failure 401 {object} ErrorResponse
// @Failure 500 {object} ErrorResponse
// @Router /auth/refresh [post]
func (h *AuthHandler) Refresh(c *gin.Context) {
	tokenStr, err := c.Cookie("refresh_token")
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "missing refresh token"})
		return
	}

	claims, err := h.jwtSvc.ValidateRefreshToken(tokenStr)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "invalid or expired refresh token"})
		return
	}

	accessToken, err := h.jwtSvc.GenerateAccessToken(claims.PlayerID, claims.Username)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not generate token"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"accessToken": accessToken})
}

// Logout godoc
// @Summary Logout
// @Description Clears the refresh token cookie for the authenticated browser session.
// @Tags auth
// @Produce json
// @Security BearerAuth
// @Success 200 {object} MessageResponse
// @Failure 401 {object} ErrorResponse
// @Router /auth/logout [post]
func (h *AuthHandler) Logout(c *gin.Context) {
	h.setRefreshCookie(c, "", -1)
	c.JSON(http.StatusOK, gin.H{"message": "logged out"})
}

// setRefreshCookie centralise la config du cookie refresh_token.
// SameSite=None en production (Render cross-origin), Lax en dev.
func (h *AuthHandler) setRefreshCookie(c *gin.Context, value string, maxAge int) {
	secure := os.Getenv("APP_ENV") == "production"
	if secure {
		c.SetSameSite(http.SameSiteNoneMode)
	} else {
		c.SetSameSite(http.SameSiteLaxMode)
	}
	c.SetCookie("refresh_token", value, maxAge, "/", "", secure, true)
}
