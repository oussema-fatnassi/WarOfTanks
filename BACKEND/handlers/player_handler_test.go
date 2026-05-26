package handlers_test

import (
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"

	"github.com/oussema-fatnassi/WarOfTanks/backend/handlers"
	"github.com/oussema-fatnassi/WarOfTanks/backend/middleware"
	"github.com/oussema-fatnassi/WarOfTanks/backend/models"
	"github.com/oussema-fatnassi/WarOfTanks/backend/services"
)

func setupPlayerRouter(db *mongo.Database) (*gin.Engine, *services.JWTService) {
	gin.SetMode(gin.TestMode)
	jwtSvc := services.NewJWTService("test-access-secret", "test-refresh-secret")
	h := handlers.NewPlayerHandler(db)

	r := gin.New()
	v1 := r.Group("/api/v1")
	protected := v1.Group("/")
	protected.Use(middleware.AuthRequired(jwtSvc))
	protected.GET("/players", h.GetPlayers)
	protected.GET("/players/me", h.GetMe)

	return r, jwtSvc
}

func seedPlayerWithStats(t *testing.T, db *mongo.Database, username string, totalScore float64) models.Player {
	t.Helper()
	now := time.Now().UTC()
	player := models.Player{
		ID:        bson.NewObjectID(),
		Username:  username,
		Stats:     models.PlayerStats{TotalScore: totalScore},
		CreatedAt: now,
		UpdatedAt: now,
	}
	_, err := db.Collection("players").InsertOne(context.Background(), player)
	if err != nil {
		t.Fatalf("seed player: %v", err)
	}
	return player
}

// ── GET /players ──────────────────────────────────────────────────────────────

func TestGetPlayers_ReturnsSortedByScore(t *testing.T) {
	db := setupTestDB(t)
	r, jwtSvc := setupPlayerRouter(db)

	seedPlayerWithStats(t, db, "low_score", 10)
	seedPlayerWithStats(t, db, "high_score", 100)
	seedPlayerWithStats(t, db, "mid_score", 50)

	token, _ := jwtSvc.GenerateAccessToken("any-id", "any-user")
	req := httptest.NewRequest(http.MethodGet, "/api/v1/players", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d: %s", w.Code, w.Body.String())
	}

	var players []map[string]interface{}
	_ = json.Unmarshal(w.Body.Bytes(), &players)

	if len(players) < 3 {
		t.Fatalf("expected at least 3 players, got %d", len(players))
	}

	score0 := players[0]["stats"].(map[string]interface{})["totalScore"].(float64)
	score1 := players[1]["stats"].(map[string]interface{})["totalScore"].(float64)
	score2 := players[2]["stats"].(map[string]interface{})["totalScore"].(float64)

	if score0 < score1 || score1 < score2 {
		t.Errorf("players not sorted by totalScore DESC: got %.0f, %.0f, %.0f", score0, score1, score2)
	}
}

func TestGetPlayers_RequiresAuth(t *testing.T) {
	db := setupTestDB(t)
	r, _ := setupPlayerRouter(db)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/players", nil)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

func TestGetPlayers_RespectsLimit(t *testing.T) {
	db := setupTestDB(t)
	r, jwtSvc := setupPlayerRouter(db)

	for i := 0; i < 5; i++ {
		seedPlayerWithStats(t, db, bson.NewObjectID().Hex(), float64(i*10))
	}

	token, _ := jwtSvc.GenerateAccessToken("any-id", "any-user")
	req := httptest.NewRequest(http.MethodGet, "/api/v1/players?limit=2", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	var players []map[string]interface{}
	_ = json.Unmarshal(w.Body.Bytes(), &players)

	if len(players) != 2 {
		t.Errorf("expected 2 players with limit=2, got %d", len(players))
	}
}

// ── GET /players/me ───────────────────────────────────────────────────────────

func TestGetMe_ReturnsOwnProfile(t *testing.T) {
	db := setupTestDB(t)
	r, jwtSvc := setupPlayerRouter(db)

	player := seedPlayerWithStats(t, db, "myself", 42)
	token, _ := jwtSvc.GenerateAccessToken(player.ID.Hex(), player.Username)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/players/me", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d: %s", w.Code, w.Body.String())
	}

	var resp map[string]interface{}
	_ = json.Unmarshal(w.Body.Bytes(), &resp)

	if resp["username"] != "myself" {
		t.Errorf("expected username 'myself', got %v", resp["username"])
	}

	if _, exists := resp["passwordHash"]; exists {
		t.Error("passwordHash must not be returned")
	}
}

func TestGetMe_ReturnsNotFound_WhenPlayerDeleted(t *testing.T) {
	db := setupTestDB(t)
	r, jwtSvc := setupPlayerRouter(db)

	fakeID := bson.NewObjectID().Hex()
	token, _ := jwtSvc.GenerateAccessToken(fakeID, "ghost")

	req := httptest.NewRequest(http.MethodGet, "/api/v1/players/me", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusNotFound {
		t.Fatalf("expected 404, got %d: %s", w.Code, w.Body.String())
	}
}
