package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson/primitive"
)

// AI represents the artificial intelligence opponent entity.
type AI struct {
	ID        primitive.ObjectID `bson:"_id,omitempty" json:"id"`
	Name      string             `bson:"name" json:"name"`
	Wins      int                `bson:"wins" json:"wins"`
	Losses    int                `bson:"losses" json:"losses"`
	CreatedAt time.Time          `bson:"createdAt" json:"createdAt"`
}