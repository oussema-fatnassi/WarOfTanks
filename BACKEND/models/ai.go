package models

import (
	"time"

	"go.mongodb.org/mongo-driver/v2/bson"
)

// AIConfig represents a behavioral configuration for the AI opponent.
type AIConfig struct {
	ID                  bson.ObjectID  `bson:"_id,omitempty" json:"id"`
	Name                string         `bson:"name" json:"name"`
	NavigationAlgorithm string         `bson:"navigationAlgorithm" json:"navigationAlgorithm"`
	CaptureSpeed        float64        `bson:"captureSpeed" json:"captureSpeed"`
	AggressionLevel     float64        `bson:"aggressionLevel" json:"aggressionLevel"`
	TankSpeed           float64        `bson:"tankSpeed" json:"tankSpeed"`
	DetectionRange      float64        `bson:"detectionRange" json:"detectionRange"`
	OwnerPlayerID       *bson.ObjectID `bson:"ownerPlayerId,omitempty" json:"ownerPlayerId,omitempty"`
	CreatedAt           time.Time      `bson:"createdAt" json:"createdAt"`
	UpdatedAt           time.Time      `bson:"updatedAt" json:"updatedAt"`
}
