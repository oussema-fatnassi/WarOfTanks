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
	"github.com/golang-jwt/jwt/v5"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"
	"golang.org/x/crypto/bcrypt"

	"github.com/oussema-fatnassi/WarOfTanks/backend/handlers"
	"github.com/oussema-fatnassi/WarOfTanks/backend/middleware"
	"github.com/oussema-fatnassi/WarOfTanks/backend/models"
	"github.com/oussema-fatnassi/WarOfTanks/backend/services"
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
	auth.POST("/refresh", h.Refresh)
	return r
}

func setupRouterWithAuth(db *mongo.Database) (*gin.Engine, *services.JWTService) {
	gin.SetMode(gin.TestMode)
	jwtSvc := services.NewJWTService("test-access-secret", "test-refresh-secret")
	h := handlers.NewAuthHandler(db, jwtSvc)

	r := gin.New()
	v1 := r.Group("/api/v1")

	authGroup := v1.Group("/auth")
	authGroup.POST("/register", h.Register)
	authGroup.POST("/login", h.Login)
	authGroup.POST("/refresh", h.Refresh)

	protected := v1.Group("/")
	protected.Use(middleware.AuthRequired(jwtSvc))
	protected.POST("/auth/logout", h.Logout)
	protected.GET("/ping", func(c *gin.Context) { c.JSON(http.StatusOK, gin.H{"ok": true}) })

	return r, jwtSvc
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
		if c.Name == "refresh_token" {
			found = true
			if !c.HttpOnly {
				t.Error("refresh_token cookie must be HttpOnly")
			}
		}
	}
	if !found {
		t.Error("refresh_token cookie not set")
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

// ── middleware tests ──────────────────────────────────────────────────────────

func TestAuthMiddleware_ValidToken(t *testing.T) {
	db := setupTestDB(t)
	r, jwtSvc := setupRouterWithAuth(db)

	token, _ := jwtSvc.GenerateAccessToken("player-id-123", "testuser")

	req := httptest.NewRequest(http.MethodGet, "/api/v1/ping", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d: %s", w.Code, w.Body.String())
	}
}

func TestAuthMiddleware_MissingToken(t *testing.T) {
	db := setupTestDB(t)
	r, _ := setupRouterWithAuth(db)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/ping", nil)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

func TestAuthMiddleware_MalformedToken(t *testing.T) {
	db := setupTestDB(t)
	r, _ := setupRouterWithAuth(db)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/ping", nil)
	req.Header.Set("Authorization", "NotBearer token")
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

func TestAuthMiddleware_ExpiredToken(t *testing.T) {
	db := setupTestDB(t)
	r, _ := setupRouterWithAuth(db)

	claims := &services.Claims{
		PlayerID: "player-id-123",
		Username: "testuser",
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(time.Now().Add(-1 * time.Hour)),
			IssuedAt:  jwt.NewNumericDate(time.Now().Add(-2 * time.Hour)),
		},
	}
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	expiredToken, _ := token.SignedString([]byte("test-access-secret"))

	req := httptest.NewRequest(http.MethodGet, "/api/v1/ping", nil)
	req.Header.Set("Authorization", "Bearer "+expiredToken)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

func TestAuthMiddleware_WrongSecret(t *testing.T) {
	db := setupTestDB(t)
	r, _ := setupRouterWithAuth(db)

	wrongSvc := services.NewJWTService("completely-wrong-secret-xyz", "irrelevant")
	token, _ := wrongSvc.GenerateAccessToken("player-id-123", "testuser")

	req := httptest.NewRequest(http.MethodGet, "/api/v1/ping", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

// ── refresh tests ─────────────────────────────────────────────────────────────

func TestRefresh_ValidCookie(t *testing.T) {
	db := setupTestDB(t)
	r, jwtSvc := setupRouterWithAuth(db)

	refreshToken, _ := jwtSvc.GenerateRefreshToken("player-id-123", "testuser")

	req := httptest.NewRequest(http.MethodPost, "/api/v1/auth/refresh", nil)
	req.AddCookie(&http.Cookie{Name: "refresh_token", Value: refreshToken})
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d: %s", w.Code, w.Body.String())
	}

	var resp map[string]string
	_ = json.Unmarshal(w.Body.Bytes(), &resp)
	if resp["accessToken"] == "" {
		t.Error("expected non-empty accessToken in response")
	}
}

func TestRefresh_MissingCookie(t *testing.T) {
	db := setupTestDB(t)
	r, _ := setupRouterWithAuth(db)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/auth/refresh", nil)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

func TestRefresh_ExpiredRefreshToken(t *testing.T) {
	db := setupTestDB(t)
	r, _ := setupRouterWithAuth(db)

	claims := &services.RefreshClaims{
		PlayerID: "player-id-123",
		Username: "testuser",
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(time.Now().Add(-1 * time.Hour)),
			IssuedAt:  jwt.NewNumericDate(time.Now().Add(-8 * 24 * time.Hour)),
		},
	}
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	expiredToken, _ := token.SignedString([]byte("test-refresh-secret"))

	req := httptest.NewRequest(http.MethodPost, "/api/v1/auth/refresh", nil)
	req.AddCookie(&http.Cookie{Name: "refresh_token", Value: expiredToken})
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", w.Code)
	}
}

// ── logout tests ──────────────────────────────────────────────────────────────

func TestLogout_ClearsCookie(t *testing.T) {
	db := setupTestDB(t)
	r, jwtSvc := setupRouterWithAuth(db)

	accessToken, _ := jwtSvc.GenerateAccessToken("player-id-123", "testuser")

	req := httptest.NewRequest(http.MethodPost, "/api/v1/auth/logout", nil)
	req.Header.Set("Authorization", "Bearer "+accessToken)
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)

	if w.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d: %s", w.Code, w.Body.String())
	}

	var found bool
	for _, c := range w.Result().Cookies() {
		if c.Name == "refresh_token" && c.MaxAge < 0 {
			found = true
		}
	}
	if !found {
		t.Error("expected refresh_token cookie to be cleared (MaxAge < 0)")
	}
}
