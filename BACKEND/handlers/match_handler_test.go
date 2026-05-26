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
