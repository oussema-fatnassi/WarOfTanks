package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson"
)

// PlayerStats holds aggregated performance data embedded inside Player.
type PlayerStats struct {
	Wins          int `bson:"wins" json:"wins"`
	Losses        int `bson:"losses" json:"losses"`
	MatchesPlayed int `bson:"matchesPlayed" json:"matchesPlayed"`
	TotalScore    int `bson:"totalScore" json:"totalScore"`
}

// Player represents a registered user in the game.
type Player struct {
	ID           bson.ObjectID `bson:"_id,omitempty" json:"id"`
	Username     string        `bson:"username" json:"username"`
	Email        string        `bson:"email" json:"email"`
	PasswordHash string        `bson:"passwordHash" json:"-"`
	RefreshToken string        `bson:"refreshToken" json:"-"`
	Stats        PlayerStats   `bson:"stats" json:"stats"`
	CreatedAt    time.Time     `bson:"createdAt" json:"createdAt"`
	UpdatedAt    time.Time     `bson:"updatedAt" json:"updatedAt"`
}
