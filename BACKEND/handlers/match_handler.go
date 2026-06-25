package handlers

import (
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
}

func NewMatchHandler(db *mongo.Database) *MatchHandler {
	return &MatchHandler{
		matches: db.Collection("matches"),
		players: db.Collection("players"),
	}
}

// SaveMatchRequest is the expected body for POST /api/v1/matches.
type SaveMatchRequest struct {
	WinnerTeam  int            `json:"winnerTeam" binding:"required"`
	PlayerScore int            `json:"playerScore"`
	AIScore     int            `json:"aiScore"`
	Duration    float64        `json:"duration"`
	AIConfigID  *bson.ObjectID `json:"aiConfigId,omitempty"`
}

// SaveMatch godoc
// POST /api/v1/matches
// Saves a match result and updates the player's stats atomically via a MongoDB transaction.
// WinnerTeam: 1 = player team wins, 2 = AI team wins.
func (h *MatchHandler) SaveMatch(c *gin.Context) {
	var req SaveMatchRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "winnerTeam is required"})
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

	if _, err := h.matches.InsertOne(ctx, match); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not save match"})
		return
	}

	statsUpdate := bson.D{{Key: "$inc", Value: bson.D{
		{Key: "stats.wins", Value: winsInc},
		{Key: "stats.losses", Value: lossesInc},
		{Key: "stats.totalMatches", Value: 1},
		{Key: "stats.totalScore", Value: float64(req.PlayerScore)},
	}}, {Key: "$set", Value: bson.D{
		{Key: "updatedAt", Value: time.Now().UTC()},
	}}}

	if _, err := h.players.UpdateOne(ctx, bson.D{{Key: "_id", Value: playerID}}, statsUpdate); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "could not update player stats"})
		return
	}

	c.JSON(http.StatusCreated, match)
}

// GetMatches godoc
// GET /api/v1/matches
// Returns the authenticated player's match history sorted by createdAt descending.
// Supports optional ?limit=10&offset=0 query params.
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
