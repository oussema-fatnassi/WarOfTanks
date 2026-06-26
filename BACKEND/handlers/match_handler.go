package handlers

import (
	"context"
	"net/http"
	"strconv"
	"time"

	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"

	"github.com/oussema-fatnassi/WarOfTanks/backend/middleware"
	"github.com/oussema-fatnassi/WarOfTanks/backend/models"
)

type MatchHandler struct {
	matches *mongo.Collection
	players *mongo.Collection
	client  *mongo.Client
}

func NewMatchHandler(db *mongo.Database, client *mongo.Client) *MatchHandler {
	return &MatchHandler{
		matches: db.Collection("matches"),
		players: db.Collection("players"),
		client:  client,
	}
}

// SaveMatchRequest is the expected body for POST /api/v1/matches.
type SaveMatchRequest struct {
	WinnerTeam  int            `json:"winnerTeam" binding:"required" example:"1"`
	PlayerScore int            `json:"playerScore" example:"1200"`
	AIScore     int            `json:"aiScore" example:"850"`
	Duration    float64        `json:"duration" example:"185.5"`
	AIConfigID  *bson.ObjectID `json:"aiConfigId,omitempty" swaggertype:"string" example:"665f1a9fbad63deb45c6e010"`
}

// SaveMatch godoc
// @Summary Save match result
// @Description Saves a match result and updates the authenticated player's stats atomically via a MongoDB transaction. `winnerTeam`: 1 = player team, 2 = AI team.
// @Tags matches
// @Accept json
// @Produce json
// @Param request body SaveMatchRequest true "Match result payload"
// @Security BearerAuth
// @Success 201 {object} models.Match
// @Failure 400 {object} ErrorResponse
// @Failure 401 {object} ErrorResponse
// @Failure 500 {object} ErrorResponse
// @Router /matches [post]
func (h *MatchHandler) SaveMatch(c *gin.Context) {
	var req SaveMatchRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "winnerTeam is required"})
		return
	}
	if errMsg := validateSaveMatchRequest(req); errMsg != "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": errMsg})
		return
	}

	playerIDStr := c.GetString(middleware.PlayerIDKey)
	playerID, err := bson.ObjectIDFromHex(playerIDStr)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "invalid player ID"})
		return
	}

	username := c.GetString(middleware.UsernameKey)

	match := models.Match{
		ID:             bson.NewObjectID(),
		PlayerID:       playerID,
		PlayerSnapshot: models.PlayerSnapshot{Username: username},
		WinnerTeam:     req.WinnerTeam,
		PlayerScore:    req.PlayerScore,
		AIScore:        req.AIScore,
		Duration:       req.Duration,
		AIConfigID:     req.AIConfigID,
		CreatedAt:      time.Now().UTC(),
	}

	winsInc, lossesInc := 0, 0
	if req.WinnerTeam == 1 {
		winsInc = 1
	} else {
		lossesInc = 1
	}

	ctx := c.Request.Context()

	session, err := h.client.StartSession()
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not start session"})
		return
	}
	defer session.EndSession(ctx)

	err = mongo.WithSession(ctx, session, func(sc context.Context) error {
		if err := session.StartTransaction(); err != nil {
			return err
		}

		if _, err := h.matches.InsertOne(sc, match); err != nil {
			_ = session.AbortTransaction(sc)
			return err
		}

		statsUpdate := bson.D{{Key: "$inc", Value: bson.D{
			{Key: "stats.wins", Value: winsInc},
			{Key: "stats.losses", Value: lossesInc},
			{Key: "stats.totalMatches", Value: 1},
			{Key: "stats.totalScore", Value: float64(req.PlayerScore)},
		}}, {Key: "$set", Value: bson.D{
			{Key: "updatedAt", Value: time.Now().UTC()},
		}}}

		if _, err := h.players.UpdateOne(sc, bson.D{{Key: "_id", Value: playerID}}, statsUpdate); err != nil {
			_ = session.AbortTransaction(sc)
			return err
		}

		return session.CommitTransaction(sc)
	})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not save match"})
		return
	}

	c.JSON(http.StatusCreated, match)
}

func validateSaveMatchRequest(req SaveMatchRequest) string {
	if req.WinnerTeam != 1 && req.WinnerTeam != 2 {
		return "winnerTeam must be 1 or 2"
	}
	if req.PlayerScore < 0 {
		return "playerScore must be greater than or equal to 0"
	}
	if req.AIScore < 0 {
		return "aiScore must be greater than or equal to 0"
	}
	if req.Duration < 0 {
		return "duration must be greater than or equal to 0"
	}
	return ""
}

// GetMatches godoc
// @Summary Get match history
// @Description Returns the authenticated player's match history sorted by creation date descending. Supports pagination with limit and offset.
// @Tags matches
// @Produce json
// @Param limit query int false "Maximum number of matches to return" minimum(1) maximum(100) default(10)
// @Param offset query int false "Number of matches to skip" minimum(0) default(0)
// @Security BearerAuth
// @Success 200 {array} models.Match
// @Failure 401 {object} ErrorResponse
// @Failure 500 {object} ErrorResponse
// @Router /matches [get]
func (h *MatchHandler) GetMatches(c *gin.Context) {
	playerIDStr := c.GetString(middleware.PlayerIDKey)
	playerID, err := bson.ObjectIDFromHex(playerIDStr)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "invalid player ID"})
		return
	}

	limit, _ := strconv.ParseInt(c.DefaultQuery("limit", "10"), 10, 64)
	offset, _ := strconv.ParseInt(c.DefaultQuery("offset", "0"), 10, 64)
	if limit <= 0 || limit > 100 {
		limit = 10
	}
	if offset < 0 {
		offset = 0
	}

	ctx := c.Request.Context()

	opts := options.Find().
		SetSort(bson.D{{Key: "createdAt", Value: -1}}).
		SetLimit(limit).
		SetSkip(offset)

	cursor, err := h.matches.Find(ctx, bson.D{{Key: "playerId", Value: playerID}}, opts)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "database error"})
		return
	}
	defer func() { _ = cursor.Close(ctx) }()

	var matches []models.Match
	if err := cursor.All(ctx, &matches); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "database error"})
		return
	}

	c.JSON(http.StatusOK, matches)
}
