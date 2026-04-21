package middleware

import (
	"net/http"
	"strings"

	"github.com/gin-gonic/gin"

	"github.com/oussema-fatnassi/WarOfTanks/backend/services"
)

const PlayerIDKey = "playerID"
const UsernameKey = "username"

// AuthRequired validates the Bearer token in the Authorization header.
// On success it sets playerID and username in the Gin context.
func AuthRequired(jwtSvc *services.JWTService) gin.HandlerFunc {
	return func(c *gin.Context) {
		header := c.GetHeader("Authorization")
		if header == "" || !strings.HasPrefix(header, "Bearer ") {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "missing or malformed authorization header"})
			return
		}

		tokenStr := strings.TrimPrefix(header, "Bearer ")
		claims, err := jwtSvc.ValidateAccessToken(tokenStr)
		if err != nil {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "invalid or expired token"})
			return
		}

		c.Set(PlayerIDKey, claims.PlayerID)
		c.Set(UsernameKey, claims.Username)
		c.Next()
	}
}
