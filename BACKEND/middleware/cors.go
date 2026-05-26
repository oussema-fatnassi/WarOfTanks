package middleware

import (
	"net/http"

	"github.com/gin-gonic/gin"
)

// CORS allows the configured frontend origin to call the API with credentials.
func CORS(frontendOrigin string) gin.HandlerFunc {
	allowedOrigins := map[string]bool{
		frontendOrigin:          true,
		"http://127.0.0.1:5173": true,
	}

	return func(c *gin.Context) {
		origin := c.Request.Header.Get("Origin")
		if allowedOrigins[origin] {
			c.Header("Access-Control-Allow-Origin", origin)
			c.Header("Access-Control-Allow-Credentials", "true")
			c.Header("Access-Control-Allow-Headers", "Content-Type, Authorization")
			c.Header("Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE, OPTIONS")
		}

		if c.Request.Method == http.MethodOptions {
			c.AbortWithStatus(http.StatusNoContent)
			return
		}

		c.Next()
	}
}
