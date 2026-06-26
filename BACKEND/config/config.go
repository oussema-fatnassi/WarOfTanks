package config

import (
	"log"
	"os"
	"strings"

	"github.com/joho/godotenv"
)

// Config holds all environment variables for the application.
type Config struct {
	MongoURI         string
	MongoDBName      string
	JWTSecret        string
	JWTRefreshSecret string
	Port             string
	FrontendOrigin   string
	AllowedOrigins   string
	AppEnv           string
	EnableSwagger    bool
}

// Load reads environment variables from .env file and returns a Config.
func Load() *Config {
	_ = godotenv.Load()

	cfg := &Config{
		MongoURI:         os.Getenv("MONGODB_URI"),
		MongoDBName:      os.Getenv("MONGODB_DB_NAME"),
		JWTSecret:        os.Getenv("JWT_SECRET"),
		JWTRefreshSecret: os.Getenv("JWT_REFRESH_SECRET"),
		Port:             os.Getenv("PORT"),
		FrontendOrigin:   os.Getenv("FRONTEND_ORIGIN"),
		AppEnv:           os.Getenv("APP_ENV"),
	}

	if cfg.MongoURI == "" || cfg.MongoDBName == "" || cfg.JWTSecret == "" || cfg.JWTRefreshSecret == "" {
		log.Fatal("❌ Missing required environment variables")
	}

	// Render injects PORT automatically; default for local runs.
	if cfg.Port == "" {
		cfg.Port = "8080"
	}

	// CORS allow-list. Prefer ALLOWED_ORIGINS (comma-separated, set per environment
	// in Render), fall back to FRONTEND_ORIGIN, then to local dev defaults.
	cfg.AllowedOrigins = os.Getenv("ALLOWED_ORIGINS")
	if cfg.AllowedOrigins == "" {
		cfg.AllowedOrigins = cfg.FrontendOrigin
	}
	if cfg.AllowedOrigins == "" {
		cfg.AllowedOrigins = "http://localhost:5173,http://localhost:3000"
	}
	if cfg.FrontendOrigin == "" {
		cfg.FrontendOrigin = "http://localhost:5173"
	}

	cfg.EnableSwagger = cfg.AppEnv != "production"
	if raw := os.Getenv("ENABLE_SWAGGER"); raw != "" {
		cfg.EnableSwagger = raw == "1" || strings.EqualFold(raw, "true")
	}

	return cfg
}
