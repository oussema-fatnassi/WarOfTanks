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
	CaptureSpeed        float64        `bson:"captureSpeed" json:"captureSpeed"`        // range: 0.5–3.0
	AggressionLevel     float64        `bson:"aggressionLevel" json:"aggressionLevel"`  // range: 0.0–1.0 (0=defensive, 1=aggressive)
	TankSpeed           float64        `bson:"tankSpeed" json:"tankSpeed"`              // range: 1.0–10.0 units/s
	DetectionRange      float64        `bson:"detectionRange" json:"detectionRange"`    // range: 5.0–50.0 Unity units
	OwnerPlayerID       *bson.ObjectID `bson:"ownerPlayerId,omitempty" json:"ownerPlayerId,omitempty"`
	CreatedAt           time.Time      `bson:"createdAt" json:"createdAt"`
	UpdatedAt           time.Time      `bson:"updatedAt" json:"updatedAt"`
}
