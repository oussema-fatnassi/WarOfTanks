package main

import (
	"context"
	"log"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/oussema-fatnassi/WarOfTanks/backend/config"
	"github.com/oussema-fatnassi/WarOfTanks/backend/handlers"
	"github.com/oussema-fatnassi/WarOfTanks/backend/middleware"
	"github.com/oussema-fatnassi/WarOfTanks/backend/services"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"
)

func main() {
	// Load environment variables
	cfg := config.Load()

	// Connect to MongoDB
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	client, err := mongo.Connect(options.Client().ApplyURI(cfg.MongoURI))
	if err != nil {
		log.Fatal("❌ Failed to connect to MongoDB:", err)
	}
	defer func() {
		if err := client.Disconnect(ctx); err != nil {
			log.Printf("Error disconnecting MongoDB: %v", err)
		}
	}()

	// Ping MongoDB to verify connection
	if err := client.Ping(ctx, nil); err != nil {
		log.Fatal("MongoDB not responding:", err)
	}
	log.Println("✅ Connected to MongoDB")

	db := client.Database(cfg.MongoDBName)

	// Initialize collections, indexes and seed data
	if err := config.InitDatabase(db); err != nil {
		log.Fatal("❌ Failed to initialize database:", err)
	}

	// Initialize services
	jwtSvc := services.NewJWTService(cfg.JWTSecret, cfg.JWTRefreshSecret)

	// Initialize handlers
	authHandler := handlers.NewAuthHandler(db, jwtSvc)

	// Initialize Gin router
	r := gin.Default()

	// Route groups
	api := r.Group("/api/v1")
	{
		api.GET("/health", func(c *gin.Context) {
			c.JSON(200, gin.H{"status": "ok"})
		})

		// Public routes
		auth := api.Group("/auth")
		{
			auth.POST("/register", authHandler.Register)
			auth.POST("/login", authHandler.Login)
		}

		// Protected routes (require valid JWT) — handlers added in follow-up issues
		_ = middleware.AuthRequired(jwtSvc)
	}

	log.Printf("🚀 Server running on port %s", cfg.Port)
	if err := r.Run(":" + cfg.Port); err != nil {
		log.Fatal("Server failed to start:", err)
	}
}