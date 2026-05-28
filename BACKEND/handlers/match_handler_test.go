package handlers_test

import (
	"bytes"
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"os"
	"testing"
	"time"

	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"

	"github.com/oussema-fatnassi/WarOfTanks/backend/handlers"
	"github.com/oussema-fatnassi/WarOfTanks/backend/middleware"
	"github.com/oussema-fatnassi/WarOfTanks/backend/models"
	"github.com/oussema-fatnassi/WarOfTanks/backend/services"
)

func setupTestDBWithClient(t *testing.T) (*mongo.Database, *mongo.Client) {
	t.Helper()
	uri := os.Getenv("MONGODB_TEST_URI")
	if uri == "" {
		uri = "mongodb://localhost:27017"
	}
	client, err := mongo.Connect(options.Client().ApplyURI(uri))
	if err != nil {
		t.Fatalf("mongo connect: %v", err)
	}
	dbName := "tankgame_test_" + bson.NewObjectID().Hex()
	db := client.Database(dbName)
	t.Cleanup(func() {
		_ = db.Drop(context.Background())
		_ = client.Disconnect(context.Background())
	})
	return db, client
}

func setupMatchRouter(db *mongo.Database, client *mongo.Client) (*gin.Engine, *services.JWTService) {
	gin.SetMode(gin.TestMode)
	jwtSvc := services.NewJWTService("test-access-secret", "test-refresh-secret")
	h := handlers.NewMatchHandler(db, client)

	r := gin.New()
	v1 := r.Group("/api/v1")
	protected := v1.Group("/")
	protected.Use(middleware.AuthRequired(jwtSvc))
	protected.POST("/matches", h.SaveMatch)
	protected.GET("/matches", h.GetMatches)

	return r, jwtSvc
}

func authPost(r *gin.Engine, path string, body interface{}, token string) *httptest.ResponseRecorder {
	b, _ := json.Marshal(body)
	req := httptest.NewRequest(http.MethodPost, path, bytes.NewReader(b))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)
	return w
}

func seedMatch(t *testing.T, db *mongo.Database, playerID bson.ObjectID, playerScore int) {
	t.Helper()
	match := models.Match{
		ID:             bson.NewObjectID(),
		PlayerID:       playerID,
		PlayerSnapshot: models.PlayerSnapshot{Username: "testuser"},
		WinnerTeam:     1,
		PlayerScore:    playerScore,
		AIScore:        50,
		Duration:       120.0,
		CreatedAt:      time.Now().UTC(),
	}
	_, err := db.Collection("matches").InsertOne(context.Background(), match)
	if err != nil {
		t.Fatalf("seed match: %v", err)
	}
}

// ── POST /matches ─────────────────────────────────────────────────────────────

func TestSaveMatch_BadRequest_MissingWinnerTeam(t *testing.T) {
	db, client := setupTestDBWithClient(t)
	r, jwtSvc := setupMatchRouter(db, client)

	token, _ := jwtSvc.GenerateAccessToken(bson.NewObjectID().Hex(), "player1")
	w := authPost(r, "/api/v1/matches", map[string]interface{}{
		"playerScore": 100,
		// winnerTeam manquant → 400
	}, token)

	if w.Code != http.StatusBadRequest {
		t.Fatalf("expected 400, got %d: %s", w.Code, w.Body.String())
	}
}

func TestSaveMatch_RequiresAuth(t *testing.T) {
	db, client := setupTestDBWithClient(t)
	r, _ := setupMatchRouter(db, client)

	w := post(r, "/api/v1/matches", map[string]interface{}{
		"winnerTeam": 1,
	})

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

// ── GET /matches ──────────────────────────────────────────────────────────────

func TestGetMatches_ReturnsOnlyOwnMatches(t *testing.T) {
	db, client := setupTestDBWithClient(t)
	r, jwtSvc := setupMatchRouter(db, client)

	playerA := bson.NewObjectID()
	playerB := bson.NewObjectID()

	seedMatch(t, db, playerA, 100)
	seedMatch(t, db, playerA, 80)
	seedMatch(t, db, playerB, 200) // autre joueur — ne doit pas apparaître

	token, _ := jwtSvc.GenerateAccessToken(playerA.Hex(), "playerA")
	req := httptest.NewRequest(http.MethodGet, "/api/v1/matches", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d: %s", w.Code, w.Body.String())
	}

	var matches []map[string]interface{}
	_ = json.Unmarshal(w.Body.Bytes(), &matches)

	if len(matches) != 2 {
		t.Errorf("expected 2 matches for playerA, got %d", len(matches))
	}
}

func TestGetMatches_RequiresAuth(t *testing.T) {
	db, client := setupTestDBWithClient(t)
	r, _ := setupMatchRouter(db, client)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/matches", nil)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

// skipIfNoReplicaSet skips the test when MongoDB is running in standalone mode.
// SaveMatch uses multi-document transactions which require a replica set.
func skipIfNoReplicaSet(t *testing.T, client *mongo.Client) {
	t.Helper()
	ctx, cancel := context.WithTimeout(context.Background(), 3*time.Second)
	defer cancel()
	if err := client.Database("admin").RunCommand(ctx, bson.D{{Key: "replSetGetStatus", Value: 1}}).Err(); err != nil {
		t.Skip("replica set not available — skipping transaction test")
	}
}

// ── POST /matches success ─────────────────────────────────────────────────────

func TestSaveMatch_Success_PlayerWins(t *testing.T) {
	db, client := setupTestDBWithClient(t)
	skipIfNoReplicaSet(t, client)
	r, jwtSvc := setupMatchRouter(db, client)

	player := seedPlayerWithStats(t, db, "winner_player", 0)
	token, _ := jwtSvc.GenerateAccessToken(player.ID.Hex(), player.Username)

	w := authPost(r, "/api/v1/matches", map[string]interface{}{
		"winnerTeam":  1,
		"playerScore": 80,
		"aiScore":     40,
		"duration":    120.5,
	}, token)

	if w.Code != http.StatusCreated {
		t.Fatalf("expected 201, got %d: %s", w.Code, w.Body.String())
	}

	var resp map[string]interface{}
	_ = json.Unmarshal(w.Body.Bytes(), &resp)
	if resp["winnerTeam"].(float64) != 1 {
		t.Errorf("expected winnerTeam 1, got %v", resp["winnerTeam"])
	}
	if resp["playerScore"].(float64) != 80 {
		t.Errorf("expected playerScore 80, got %v", resp["playerScore"])
	}

	var updated models.Player
	if err := db.Collection("players").FindOne(context.Background(), bson.D{{Key: "_id", Value: player.ID}}).Decode(&updated); err != nil {
		t.Fatalf("player not found after match save: %v", err)
	}
	if updated.Stats.Wins != 1 {
		t.Errorf("expected wins=1, got %d", updated.Stats.Wins)
	}
	if updated.Stats.Losses != 0 {
		t.Errorf("expected losses=0, got %d", updated.Stats.Losses)
	}
	if updated.Stats.TotalMatches != 1 {
		t.Errorf("expected totalMatches=1, got %d", updated.Stats.TotalMatches)
	}
	if updated.Stats.TotalScore != 80 {
		t.Errorf("expected totalScore=80, got %.0f", updated.Stats.TotalScore)
	}
}

func TestSaveMatch_Success_AIWins(t *testing.T) {
	db, client := setupTestDBWithClient(t)
	skipIfNoReplicaSet(t, client)
	r, jwtSvc := setupMatchRouter(db, client)

	player := seedPlayerWithStats(t, db, "loser_player", 0)
	token, _ := jwtSvc.GenerateAccessToken(player.ID.Hex(), player.Username)

	w := authPost(r, "/api/v1/matches", map[string]interface{}{
		"winnerTeam":  2,
		"playerScore": 20,
		"aiScore":     100,
		"duration":    90.0,
	}, token)

	if w.Code != http.StatusCreated {
		t.Fatalf("expected 201, got %d: %s", w.Code, w.Body.String())
	}

	var updated models.Player
	if err := db.Collection("players").FindOne(context.Background(), bson.D{{Key: "_id", Value: player.ID}}).Decode(&updated); err != nil {
		t.Fatalf("player not found after match save: %v", err)
	}
	if updated.Stats.Losses != 1 {
		t.Errorf("expected losses=1, got %d", updated.Stats.Losses)
	}
	if updated.Stats.Wins != 0 {
		t.Errorf("expected wins=0, got %d", updated.Stats.Wins)
	}
	if updated.Stats.TotalScore != 20 {
		t.Errorf("expected totalScore=20, got %.0f", updated.Stats.TotalScore)
	}
}

func TestGetMatches_RespectsLimit(t *testing.T) {
	db, client := setupTestDBWithClient(t)
	r, jwtSvc := setupMatchRouter(db, client)

	playerID := bson.NewObjectID()
	seedMatch(t, db, playerID, 10)
	seedMatch(t, db, playerID, 20)
	seedMatch(t, db, playerID, 30)

	token, _ := jwtSvc.GenerateAccessToken(playerID.Hex(), "player")
	req := httptest.NewRequest(http.MethodGet, "/api/v1/matches?limit=2", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	var matches []map[string]interface{}
	_ = json.Unmarshal(w.Body.Bytes(), &matches)

	if len(matches) != 2 {
		t.Errorf("expected 2 matches with limit=2, got %d", len(matches))
	}
}
