package middleware_test

import (
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/gin-gonic/gin"

	"github.com/oussema-fatnassi/WarOfTanks/backend/middleware"
)

func setupCORSRouter(frontendOrigin string) *gin.Engine {
	gin.SetMode(gin.TestMode)
	r := gin.New()
	r.Use(middleware.CORS(frontendOrigin))
	r.GET("/ping", func(c *gin.Context) {
		c.Status(http.StatusOK)
	})
	return r
}

func doRequest(r *gin.Engine, method, origin string) *httptest.ResponseRecorder {
	req := httptest.NewRequest(method, "/ping", nil)
	if origin != "" {
		req.Header.Set("Origin", origin)
	}
	w := httptest.NewRecorder()
	r.ServeHTTP(w, req)
	return w
}

// An allowed origin gets the headers that let the browser actually read the response.
func TestCORS_AllowedOrigin_SetsHeaders(t *testing.T) {
	r := setupCORSRouter("http://localhost:3000")
	w := doRequest(r, http.MethodGet, "http://localhost:3000")

	if w.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d", w.Code)
	}
	if got := w.Header().Get("Access-Control-Allow-Origin"); got != "http://localhost:3000" {
		t.Errorf("expected Access-Control-Allow-Origin=http://localhost:3000, got %q", got)
	}
	if got := w.Header().Get("Access-Control-Allow-Credentials"); got != "true" {
		t.Errorf("expected Access-Control-Allow-Credentials=true, got %q", got)
	}
}

// A disallowed origin gets no CORS headers, so the browser blocks the page's JS from
// reading the response even though the server still processed the request — CORS is
// enforced client-side, the API can't refuse the request itself, only withhold the
// headers that make the response legible cross-origin.
func TestCORS_DisallowedOrigin_NoAccessControlHeaders(t *testing.T) {
	r := setupCORSRouter("http://localhost:3000")
	w := doRequest(r, http.MethodGet, "http://evil.example.com")

	if w.Code != http.StatusOK {
		t.Fatalf("expected the request to still reach the handler (200), got %d", w.Code)
	}
	if got := w.Header().Get("Access-Control-Allow-Origin"); got != "" {
		t.Errorf("expected no Access-Control-Allow-Origin for a disallowed origin, got %q", got)
	}
}

// A CORS preflight (OPTIONS) from an allowed origin short-circuits with 204 and still
// carries the headers the browser needs before it sends the real request.
func TestCORS_Preflight_ReturnsNoContentWithHeaders(t *testing.T) {
	r := setupCORSRouter("http://localhost:3000")
	w := doRequest(r, http.MethodOptions, "http://localhost:3000")

	if w.Code != http.StatusNoContent {
		t.Fatalf("expected 204 for a preflight request, got %d", w.Code)
	}
	if got := w.Header().Get("Access-Control-Allow-Methods"); got == "" {
		t.Error("expected Access-Control-Allow-Methods to be set on the preflight response")
	}
}

// The local Vite dev origin is always allowed, even if it isn't in the configured
// FRONTEND_ORIGIN — a deliberate fallback so local development keeps working regardless
// of what's set in .env.
func TestCORS_ViteDevOrigin_AlwaysAllowed(t *testing.T) {
	r := setupCORSRouter("https://war-of-tanks.vercel.app")
	w := doRequest(r, http.MethodGet, "http://127.0.0.1:5173")

	if got := w.Header().Get("Access-Control-Allow-Origin"); got != "http://127.0.0.1:5173" {
		t.Errorf("expected the default Vite origin to always be allowed, got %q", got)
	}
}
