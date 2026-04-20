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
	"golang.org/x/crypto/bcrypt"

	"yourmodule/handlers"
	"yourmodule/models"
	"yourmodule/services"
)

// ── helpers ──────────────────────────────────────────────────────────────────

func setupTestDB(t *testing.T) *mongo.Database {
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
	return db
}

func setupRouter(db *mongo.Database) *gin.Engine {
	gin.SetMode(gin.TestMode)
	jwtSvc := services.NewJWTService("test-access-secret", "test-refresh-secret")
	h := handlers.NewAuthHandler(db, jwtSvc)

	r := gin.New()
	v1 := r.Group("/api/v1")
	auth := v1.Group("/auth")
	auth.POST("/register", h.Register)
	auth.POST("/login", h.Login)
	return r
}

func post(r *gin.Engine, path string, body interface{}) *httptest.ResponseRecorder {
	b, _ := json.Marshal(body)
	req := httptest.NewRequest(http.MethodPost, path, bytes.NewReader(b))
	req.Header.Set("Content-Type", "application/json")
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)
	return w
}

// ── register tests ────────────────────────────────────────────────────────────

func TestRegister_Success(t *testing.T) {
	db := setupTestDB(t)
	r := setupRouter(db)

	w := post(r, "/api/v1/auth/register", map[string]string{
		"username": "alice_01",
		"password": "securepass",
	})

	if w.Code != http.StatusCreated {
		t.Fatalf("expected 201, got %d: %s", w.Code, w.Body.String())
	}

	var resp map[string]string
	_ = json.Unmarshal(w.Body.Bytes(), &resp)

	if resp["username"] != "alice_01" {
		t.Errorf("expected username alice_01, got %s", resp["username"])
	}
	if resp["id"] == "" {
		t.Error("expected non-empty id")
	}

	// Verify password is hashed in DB
	var stored models.Player
	err := db.Collection("players").FindOne(context.Background(), bson.D{{Key: "username", Value: "alice_01"}}).Decode(&stored)
	if err != nil {
		t.Fatalf("player not found in db: %v", err)
	}
	if stored.PasswordHash == "securepass" {
		t.Error("password was stored in plain text")
	}
	if bcrypt.CompareHashAndPassword([]byte(stored.PasswordHash), []byte("securepass")) != nil {
		t.Error("stored hash does not match plain password")
	}
}

func TestRegister_DuplicateUsername(t *testing.T) {
	db := setupTestDB(t)
	r := setupRouter(db)

	body := map[string]string{"username": "bob", "password": "password123"}
	post(r, "/api/v1/auth/register", body) // first registration

	w := post(r, "/api/v1/auth/register", body) // duplicate
	if w.Code != http.StatusConflict {
		t.Fatalf("expected 409, got %d: %s", w.Code, w.Body.String())
	}
}

func TestRegister_InvalidUsername(t *testing.T) {
	db := setupTestDB(t)
	r := setupRouter(db)

	cases := []string{"ab", "toolongusername1234567", "bad name!", "has-hyphen"}
	for _, u := range cases {
		w := post(r, "/api/v1/auth/register", map[string]string{"username": u, "password": "password123"})
		if w.Code != http.StatusBadRequest {
			t.Errorf("username %q: expected 400, got %d", u, w.Code)
		}
	}
}

func TestRegister_ShortPassword(t *testing.T) {
	db := setupTestDB(t)
	r := setupRouter(db)

	w := post(r, "/api/v1/auth/register", map[string]string{
		"username": "validuser",
		"password": "short",
	})
	if w.Code != http.StatusBadRequest {
		t.Fatalf("expected 400, got %d: %s", w.Code, w.Body.String())
	}
}

// ── login tests ───────────────────────────────────────────────────────────────

func seedPlayer(t *testing.T, db *mongo.Database, username, password string) {
	t.Helper()
	hash, _ := bcrypt.GenerateFromPassword([]byte(password), bcrypt.DefaultCost)
	now := time.Now().UTC()
	player := models.Player{
		ID:           bson.NewObjectID(),
		Username:     username,
		PasswordHash: string(hash),
		CreatedAt:    now,
		UpdatedAt:    now,
	}
	_, err := db.Collection("players").InsertOne(context.Background(), player)
	if err != nil {
		t.Fatalf("seed player: %v", err)
	}
}

func TestLogin_Success(t *testing.T) {
	db := setupTestDB(t)
	r := setupRouter(db)
	seedPlayer(t, db, "charlie", "mypassword")

	w := post(r, "/api/v1/auth/login", map[string]string{
		"username": "charlie",
		"password": "mypassword",
	})

	if w.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d: %s", w.Code, w.Body.String())
	}

	var resp map[string]interface{}
	_ = json.Unmarshal(w.Body.Bytes(), &resp)

	if resp["accessToken"] == "" || resp["accessToken"] == nil {
		t.Error("expected non-empty accessToken")
	}

	// Verify HttpOnly refresh token cookie is set
	cookies := w.Result().Cookies()
	var found bool
	for _, c := range cookies {
		if c.Name == "refreshToken" {
			found = true
			if !c.HttpOnly {
				t.Error("refreshToken cookie must be HttpOnly")
			}
		}
	}
	if !found {
		t.Error("refreshToken cookie not set")
	}

	// Ensure passwordHash is not in the player object
	if player, ok := resp["player"].(map[string]interface{}); ok {
		if _, exists := player["passwordHash"]; exists {
			t.Error("passwordHash must not be returned in response")
		}
	}
}

func TestLogin_WrongPassword(t *testing.T) {
	db := setupTestDB(t)
	r := setupRouter(db)
	seedPlayer(t, db, "dave", "correctpass")

	w := post(r, "/api/v1/auth/login", map[string]string{
		"username": "dave",
		"password": "wrongpass",
	})

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d: %s", w.Code, w.Body.String())
	}

	var resp map[string]string
	_ = json.Unmarshal(w.Body.Bytes(), &resp)
	if resp["error"] != "invalid credentials" {
		t.Errorf("expected generic error message, got %q", resp["error"])
	}
}

func TestLogin_UnknownUsername(t *testing.T) {
	db := setupTestDB(t)
	r := setupRouter(db)

	w := post(r, "/api/v1/auth/login", map[string]string{
		"username": "nobody",
		"password": "password123",
	})

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d: %s", w.Code, w.Body.String())
	}
}