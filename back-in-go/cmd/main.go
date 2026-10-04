package main

import (
	"os"
	"time"

	"github.com/gin-gonic/gin"
	"golang.org/x/time/rate"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/database"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/handler"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/handler/middleware"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/repository"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/service"
)

func main() {
	router := gin.Default()
	// Permitir em média 3 requisições por minuto, com pico (burst) de até 5 requisições instantâneas por IP
	limiter := middleware.NewRateLimiter(rate.Every(time.Minute/3), 5)
	router.Use(limiter.Middleware(), middleware.ErrorHandler())

	db, _ := database.ConnectDB()

	// Auth Resources
	userRepository := repository.NewUserRepository(db)
	authService := service.NewAuthService(
		userRepository,
		os.Getenv("HASH_SECRET"),
	)
	authHandler := handler.NewAuthHandler(authService)

	// Stock Resources
	stockRepo := repository.NewStockRepository(db)
	stockService := service.NewStockService(stockRepo)
	stockHandler := handler.NewStockHandler(stockService)

	v1 := router.Group("/api/v1")
	{
		authorization := v1.Group("/auth")
		{
			authorization.POST("/login", authHandler.Login)
			authorization.POST("/register", authHandler.CreateUser)
		}

		stock := v1.Group("/stock")
		{
			stock.GET("/list-all-items", stockHandler.GetAllItems)
		}
	}

	router.Run(":8080")
}
