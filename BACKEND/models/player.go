package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson"
)

// PlayerStats holds aggregated performance data embedded inside Player.
type PlayerStats struct {
	Wins         int     `bson:"wins" json:"wins" example:"7"`
	Losses       int     `bson:"losses" json:"losses" example:"3"`
	TotalMatches int     `bson:"totalMatches" json:"totalMatches" example:"10"`
	TotalScore   float64 `bson:"totalScore" json:"totalScore" example:"12450"`
}

// Player represents a registered user in the game.
type Player struct {
	ID           bson.ObjectID `bson:"_id,omitempty" json:"id" swaggertype:"string" example:"665f1a9fbad63deb45c6e001"`
	Username     string        `bson:"username" json:"username" example:"tank_commander"`
	Email        string        `bson:"email" json:"email" example:"tank@example.com"`
	PasswordHash string        `bson:"passwordHash" json:"-" swaggerignore:"true"`
	RefreshToken string        `bson:"refreshToken" json:"-" swaggerignore:"true"`
	Stats        PlayerStats   `bson:"stats" json:"stats"`
	CreatedAt    time.Time     `bson:"createdAt" json:"createdAt" example:"2026-06-26T10:00:00Z"`
	UpdatedAt    time.Time     `bson:"updatedAt" json:"updatedAt" example:"2026-06-26T10:05:00Z"`
}
