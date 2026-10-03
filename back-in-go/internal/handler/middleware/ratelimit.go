package middleware

import (
	"net/http"
	"sync"
	"time"

	"github.com/gin-gonic/gin"
	"golang.org/x/time/rate"
)

type IPClient struct {
	limiter  *rate.Limiter
	lastSeen time.Time
}

type RateLimiter struct {
	mu      sync.RWMutex
	clients map[string]*IPClient
	r       rate.Limit
	b       int
}

func NewRateLimiter(r rate.Limit, b int) *RateLimiter {
	rl := &RateLimiter{
		clients: make(map[string]*IPClient),
		r:       r,
		b:       b,
	}

	return rl
}

func (rl *RateLimiter) getClientLimiter(ip string) *rate.Limiter {
	rl.mu.Lock()
	defer rl.mu.Unlock()

	client, exists := rl.clients[ip]
	if !exists {
		limiter := rate.NewLimiter(rl.r, rl.b)
		rl.clients[ip] = &IPClient{
			limiter:  limiter,
			lastSeen: time.Now(),
		}

		return limiter
	}

	client.lastSeen = time.Now()
	return client.limiter
}

func (rl *RateLimiter) Middleware() gin.HandlerFunc {
	return func(c *gin.Context) {
		ip := c.ClientIP()
		limiter := rl.getClientLimiter(ip)

		if !limiter.Allow() {
			c.AbortWithStatusJSON(http.StatusTooManyRequests, gin.H{
				"error": "Você excedeu o limite de requisições. Tente novamente mais tarde",
			})
			return
		}

		c.Next()
	}
}

func (rl *RateLimiter) cleanupClients() {
	for {
		time.Sleep(3 * time.Minute)

		rl.mu.Lock()
		defer rl.mu.Unlock()
		for ip, client := range rl.clients {
			if time.Since(client.lastSeen) > 3*time.Minute {
				delete(rl.clients, ip)
			}
		}
	}
}
