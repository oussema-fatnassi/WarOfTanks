package handlers

import (
	"errors"
	"net/http"
	"os"
	"regexp"
	"time"

	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"golang.org/x/crypto/bcrypt"

	"yourmodule/models"
	"yourmodule/services"
)

var usernameRegex = regexp.MustCompile(`^[a-zA-Z0-9_]{3,20}$`)

type AuthHandler struct {
	players  *mongo.Collection
	jwtSvc   *services.JWTService
}

func NewAuthHandler(db *mongo.Database, jwtSvc *services.JWTService) *AuthHandler {
	return &AuthHandler{
		players: db.Collection("players"),
		jwtSvc:  jwtSvc,
	}
}

// RegisterRequest is the expected body for POST /auth/register.
type RegisterRequest struct {
	Username string `json:"username" binding:"required"`
	Password string `json:"password" binding:"required"`
}

// LoginRequest is the expected body for POST /auth/login.
type LoginRequest struct {
	Username string `json:"username" binding:"required"`
	Password string `json:"password" binding:"required"`
}

// Register godoc
// POST /api/v1/auth/register
func (h *AuthHandler) Register(c *gin.Context) {
	var req RegisterRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "username and password are required"})
		return
	}

	// Validate username
	if !usernameRegex.MatchString(req.Username) {
		c.JSON(http.StatusBadRequest, gin.H{"error": "username must be 3–20 characters (letters, digits, underscore only)"})
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
		Email:        "", // not required at registration in this task
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
	})
}

// Login godoc
// POST /api/v1/auth/login
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

	refreshToken, err := h.jwtSvc.GenerateRefreshToken(player.ID.Hex())
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not generate token"})
		return
	}

	// Persist refresh token hash in DB (optional: store raw for simplicity here)
	_, _ = h.players.UpdateOne(ctx,
		bson.D{{Key: "_id", Value: player.ID}},
		bson.D{{Key: "$set", Value: bson.D{
			{Key: "refreshToken", Value: refreshToken},
			{Key: "updatedAt", Value: time.Now().UTC()},
		}}},
	)

	secure := os.Getenv("APP_ENV") == "production"

	c.SetCookie(
		"refreshToken",
		refreshToken,
		int((7 * 24 * time.Hour).Seconds()),
		"/",
		"",
		secure,
		true, // HttpOnly
	)

	// SameSite=Strict must be set manually (Gin's SetCookie doesn't expose it)
	c.Header("Set-Cookie", c.Writer.Header().Get("Set-Cookie")+"; SameSite=Strict")

	c.JSON(http.StatusOK, gin.H{
		"accessToken": accessToken,
		"player":      player, // passwordHash excluded by json:"-"
	})
}