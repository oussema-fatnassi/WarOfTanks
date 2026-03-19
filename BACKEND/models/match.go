package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson/primitive"
)

// Match represents a completed game between a player and the AI.
type Match struct {
	ID         primitive.ObjectID `bson:"_id,omitempty" json:"id"`
	PlayerID   primitive.ObjectID `bson:"playerId" json:"playerId"`
	WinnerTeam int                `bson:"winnerTeam" json:"winnerTeam"`
	Duration   float64            `bson:"duration" json:"duration"`
	CreatedAt  time.Time          `bson:"createdAt" json:"createdAt"`
}