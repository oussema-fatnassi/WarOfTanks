package main

import (
	"context"
	"log"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/oussema-fatnassi/WarOfTanks/backend/config"
	_ "github.com/oussema-fatnassi/WarOfTanks/backend/docs"
	"github.com/oussema-fatnassi/WarOfTanks/backend/handlers"
	"github.com/oussema-fatnassi/WarOfTanks/backend/middleware"
	"github.com/oussema-fatnassi/WarOfTanks/backend/services"
	swaggerFiles "github.com/swaggo/files"
	ginSwagger "github.com/swaggo/gin-swagger"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"
)

// @title War of Tanks API
// @version 1.0
// @description REST API for War of Tanks authentication, leaderboard, player stats, and match history.
// @description The access token is sent as `Authorization: Bearer <token>`. The refresh token is stored in an HttpOnly cookie and is never returned in JSON.
// @BasePath /api/v1
// @securityDefinitions.apikey BearerAuth
// @in header
// @name Authorization
// @description Type "Bearer " followed by a valid JWT access token.
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
	playerHandler := handlers.NewPlayerHandler(db)
	matchHandler := handlers.NewMatchHandler(db, client)

	// Initialize Gin router
	r := gin.Default()
	r.Use(middleware.CORS(cfg.AllowedOrigins))

	if cfg.EnableSwagger {
		r.GET("/swagger/*any", ginSwagger.WrapHandler(swaggerFiles.Handler))
	}

	// Root health check (used by Render's health check and uptime probes).
	r.GET("/health", healthHandler)

	// Route groups
	api := r.Group("/api/v1")
	{
		api.GET("/health", healthHandler)

		// Public routes
		auth := api.Group("/auth")
		{
			auth.POST("/register", authHandler.Register)
			auth.POST("/login", authHandler.Login)
			auth.POST("/refresh", authHandler.Refresh)
		}

		// Protected routes
		protected := api.Group("/")
		protected.Use(middleware.AuthRequired(jwtSvc))
		{
			protected.POST("/auth/logout", authHandler.Logout)

			protected.GET("/players", playerHandler.GetPlayers)
			protected.GET("/players/me", playerHandler.GetMe)

			protected.POST("/matches", matchHandler.SaveMatch)
			protected.GET("/matches", matchHandler.GetMatches)
		}
	}

	log.Printf("🚀 Server running on port %s", cfg.Port)
	if err := r.Run(":" + cfg.Port); err != nil {
		log.Fatal("Server failed to start:", err)
	}
}

// healthHandler godoc
// @Summary Health check
// @Description Returns the API health status. The same handler is also mounted at `/health` for Render health checks.
// @Tags health
// @Produce json
// @Success 200 {object} handlers.HealthResponse
// @Router /health [get]
func healthHandler(c *gin.Context) {
	c.JSON(http.StatusOK, handlers.HealthResponse{Status: "ok"})
}
