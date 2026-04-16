package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
)

// Config holds all environment variables for the application.
type Config struct {
	MongoURI         string
	MongoDBName      string
	JWTSecret        string
	JWTRefreshSecret string
	Port             string
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
	}

	if cfg.MongoURI == "" || cfg.JWTSecret == "" || cfg.MongoDBName == "" {
		log.Fatal("❌ Missing required environment variables")
	}

	return cfg
}
