package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson"
)

// PlayerSnapshot captures the player's username at the time of the match.
type PlayerSnapshot struct {
	Username string `bson:"username" json:"username" example:"tank_commander"`
}

// Match represents a completed game between a player and the AI.
type Match struct {
	ID             bson.ObjectID  `bson:"_id,omitempty" json:"id" swaggertype:"string" example:"665f1a9fbad63deb45c6e020"`
	PlayerID       bson.ObjectID  `bson:"playerId" json:"playerId" swaggertype:"string" example:"665f1a9fbad63deb45c6e001"`
	PlayerSnapshot PlayerSnapshot `bson:"playerSnapshot" json:"playerSnapshot"`
	WinnerTeam     int            `bson:"winnerTeam" json:"winnerTeam" example:"1"` // 1 = player team, 2 = AI team
	PlayerScore    int            `bson:"playerScore" json:"playerScore" example:"1200"`
	AIScore        int            `bson:"aiScore" json:"aiScore" example:"850"`
	Duration       float64        `bson:"duration" json:"duration" example:"185.5"`
	AIConfigID     *bson.ObjectID `bson:"aiConfigId,omitempty" json:"aiConfigId,omitempty" swaggertype:"string" example:"665f1a9fbad63deb45c6e010"`
	CreatedAt      time.Time      `bson:"createdAt" json:"createdAt" example:"2026-06-26T10:00:00Z"`
}
