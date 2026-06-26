package handlers

import (
	"errors"
	"net/http"
	"strconv"

	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"

	"github.com/oussema-fatnassi/WarOfTanks/backend/middleware"
	"github.com/oussema-fatnassi/WarOfTanks/backend/models"
)

type PlayerHandler struct {
	players *mongo.Collection
}

func NewPlayerHandler(db *mongo.Database) *PlayerHandler {
	return &PlayerHandler{players: db.Collection("players")}
}

// GetPlayers godoc
// @Summary Get leaderboard
// @Description Returns players sorted by total score descending. Supports pagination with limit and offset.
// @Tags players
// @Produce json
// @Param limit query int false "Maximum number of players to return" minimum(1) maximum(100) default(20)
// @Param offset query int false "Number of players to skip" minimum(0) default(0)
// @Security BearerAuth
// @Success 200 {array} models.Player
// @Failure 401 {object} ErrorResponse
// @Failure 500 {object} ErrorResponse
// @Router /players [get]
func (h *PlayerHandler) GetPlayers(c *gin.Context) {
	limit, _ := strconv.ParseInt(c.DefaultQuery("limit", "20"), 10, 64)
	offset, _ := strconv.ParseInt(c.DefaultQuery("offset", "0"), 10, 64)

	if limit <= 0 || limit > 100 {
		limit = 20
	}
	if offset < 0 {
		offset = 0
	}

	ctx := c.Request.Context()

	opts := options.Find().
		SetSort(bson.D{{Key: "stats.totalScore", Value: -1}}).
		SetLimit(limit).
		SetSkip(offset)

	cursor, err := h.players.Find(ctx, bson.D{}, opts)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "database error"})
		return
	}
	defer func() { _ = cursor.Close(ctx) }()

	var players []models.Player
	if err := cursor.All(ctx, &players); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "database error"})
		return
	}

	c.JSON(http.StatusOK, players)
}

// GetMe godoc
// @Summary Get current player
// @Description Returns the authenticated player's profile using the player ID from the JWT claims.
// @Tags players
// @Produce json
// @Security BearerAuth
// @Success 200 {object} models.Player
// @Failure 401 {object} ErrorResponse
// @Failure 404 {object} ErrorResponse
// @Failure 500 {object} ErrorResponse
// @Router /players/me [get]
func (h *PlayerHandler) GetMe(c *gin.Context) {
	playerIDStr := c.GetString(middleware.PlayerIDKey)
	playerID, err := bson.ObjectIDFromHex(playerIDStr)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "invalid player ID"})
		return
	}

	ctx := c.Request.Context()

	var player models.Player
	err = h.players.FindOne(ctx, bson.D{{Key: "_id", Value: playerID}}).Decode(&player)
	if errors.Is(err, mongo.ErrNoDocuments) {
		c.JSON(http.StatusNotFound, gin.H{"error": "player not found"})
		return
	}
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "database error"})
		return
	}

	c.JSON(http.StatusOK, player)
}
