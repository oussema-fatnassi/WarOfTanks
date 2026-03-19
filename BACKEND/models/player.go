package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson/primitive"
)

// Player represents a registered user in the game.
type Player struct {
	ID           primitive.ObjectID `bson:"_id,omitempty" json:"id"`
	Username     string             `bson:"username" json:"username"`
	Email        string             `bson:"email" json:"email"`
	PasswordHash string             `bson:"passwordHash" json:"-"`
	RefreshToken string             `bson:"refreshToken" json:"-"`
	CreatedAt    time.Time          `bson:"createdAt" json:"createdAt"`
}