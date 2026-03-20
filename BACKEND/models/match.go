package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson"
)

// Match represents a completed game between a player and the AI.
type Match struct {
	ID         bson.ObjectID `bson:"_id,omitempty" json:"id"`
	PlayerID   bson.ObjectID `bson:"playerId" json:"playerId"`
	WinnerTeam int                `bson:"winnerTeam" json:"winnerTeam"`
	Duration   float64            `bson:"duration" json:"duration"`
	CreatedAt  time.Time          `bson:"createdAt" json:"createdAt"`
}