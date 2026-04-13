package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson"
)

// PlayerSnapshot captures the player's username at the time of the match.
type PlayerSnapshot struct {
	Username string `bson:"username" json:"username"`
}

// Match represents a completed game between a player and the AI.
type Match struct {
	ID             bson.ObjectID    `bson:"_id,omitempty" json:"id"`
	PlayerID       bson.ObjectID    `bson:"playerId" json:"playerId"`
	PlayerSnapshot PlayerSnapshot   `bson:"playerSnapshot" json:"playerSnapshot"`
	WinnerTeam     int              `bson:"winnerTeam" json:"winnerTeam"` // 1 = player team, 2 = AI team
	PlayerScore    int              `bson:"playerScore" json:"playerScore"`
	AIScore        int              `bson:"aiScore" json:"aiScore"`
	Duration       float64          `bson:"duration" json:"duration"`
	AIConfigID     *bson.ObjectID   `bson:"aiConfigId,omitempty" json:"aiConfigId,omitempty"`
	CreatedAt      time.Time        `bson:"createdAt" json:"createdAt"`
}