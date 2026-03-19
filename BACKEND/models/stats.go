package models

import "go.mongodb.org/mongo-driver/v2/bson"

// Stats holds the aggregated performance data for a player.
type Stats struct {
	ID            primitive.ObjectID `bson:"_id,omitempty" json:"id"`
	PlayerID      primitive.ObjectID `bson:"playerId" json:"playerId"`
	Wins          int                `bson:"wins" json:"wins"`
	Losses        int                `bson:"losses" json:"losses"`
	MatchesPlayed int                `bson:"matchesPlayed" json:"matchesPlayed"`
}