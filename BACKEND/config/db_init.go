package config

import (
	"context"
	"log"
	"time"

	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"
)

// InitDatabase creates collections, applies indexes, and seeds default AI configs.
// It is idempotent: safe to call on every server startup.
func InitDatabase(db *mongo.Database) error {
	ctx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
	defer cancel()

	if err := createCollections(ctx, db); err != nil {
		return err
	}
	if err := createIndexes(ctx, db); err != nil {
		return err
	}
	if err := seedAIConfigs(ctx, db); err != nil {
		return err
	}

	log.Println("✅ Database initialization complete")
	return nil
}

func createCollections(ctx context.Context, db *mongo.Database) error {
	collections := []string{"players", "matches", "aiConfigs"}

	existing, err := db.ListCollectionNames(ctx, bson.D{})
	if err != nil {
		return err
	}
	existingSet := make(map[string]bool, len(existing))
	for _, name := range existing {
		existingSet[name] = true
	}

	for _, name := range collections {
		if existingSet[name] {
			log.Printf("[DB] Collection already exists, skipping: %s", name)
			continue
		}
		if err := db.CreateCollection(ctx, name); err != nil {
			return err
		}
		log.Printf("[DB] Created collection: %s", name)
	}
	return nil
}

func createIndexes(ctx context.Context, db *mongo.Database) error {
	// --- players ---
	playersCol := db.Collection("players")
	_, err := playersCol.Indexes().CreateMany(ctx, []mongo.IndexModel{
		{
			Keys:    bson.D{{Key: "username", Value: 1}},
			Options: options.Index().SetUnique(true).SetName("unique_username"),
		},
		{
			Keys:    bson.D{{Key: "email", Value: 1}},
			Options: options.Index().SetUnique(true).SetName("unique_email"),
		},
		{
			Keys:    bson.D{{Key: "stats.totalScore", Value: -1}},
			Options: options.Index().SetName("leaderboard_total_score"),
		},
	})
	if err != nil {
		return err
	}
	log.Println("[DB] Indexes applied: players")

	// --- matches ---
	matchesCol := db.Collection("matches")
	_, err = matchesCol.Indexes().CreateMany(ctx, []mongo.IndexModel{
		{
			Keys:    bson.D{{Key: "playerId", Value: 1}},
			Options: options.Index().SetName("idx_player_id"),
		},
		{
			Keys:    bson.D{{Key: "createdAt", Value: -1}},
			Options: options.Index().SetName("idx_created_at_desc"),
		},
	})
	if err != nil {
		return err
	}
	log.Println("[DB] Indexes applied: matches")

	// --- aiConfigs ---
	aiConfigsCol := db.Collection("aiConfigs")
	_, err = aiConfigsCol.Indexes().CreateMany(ctx, []mongo.IndexModel{
		{
			Keys:    bson.D{{Key: "ownerPlayerId", Value: 1}},
			Options: options.Index().SetName("idx_owner_player_id"),
		},
	})
	if err != nil {
		return err
	}
	log.Println("[DB] Indexes applied: aiConfigs")

	return nil
}

var defaultAIConfigs = []struct {
	Name                string
	NavigationAlgorithm string
	CaptureSpeed        float64
	AggressionLevel     float64
	TankSpeed           float64
	DetectionRange      float64
}{
	{"Aggressive", "AStar", 2.5, 0.9, 7.0, 35.0},
	{"Balanced", "AStar", 1.5, 0.5, 5.0, 25.0},
	{"Defensive", "Dijkstra", 0.8, 0.1, 3.5, 20.0},
}

func seedAIConfigs(ctx context.Context, db *mongo.Database) error {
	col := db.Collection("aiConfigs")
	now := time.Now().UTC()

	for _, cfg := range defaultAIConfigs {
		filter := bson.D{{Key: "name", Value: cfg.Name}, {Key: "ownerPlayerId", Value: nil}}

		var existing bson.M
		err := col.FindOne(ctx, filter).Decode(&existing)
		if err == nil {
			log.Printf("[DB] AI config already exists, skipping seed: %s", cfg.Name)
			continue
		}
		if err != mongo.ErrNoDocuments {
			return err
		}

		doc := bson.D{
			{Key: "name", Value: cfg.Name},
			{Key: "navigationAlgorithm", Value: cfg.NavigationAlgorithm},
			{Key: "captureSpeed", Value: cfg.CaptureSpeed},
			{Key: "aggressionLevel", Value: cfg.AggressionLevel},
			{Key: "tankSpeed", Value: cfg.TankSpeed},
			{Key: "detectionRange", Value: cfg.DetectionRange},
			{Key: "ownerPlayerId", Value: nil},
			{Key: "createdAt", Value: now},
			{Key: "updatedAt", Value: now},
		}

		if _, err := col.InsertOne(ctx, doc); err != nil {
			return err
		}
		log.Printf("[DB] Seeded AI config: %s", cfg.Name)
	}
	return nil
}