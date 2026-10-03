package middleware

import (
	"errors"
	"log"
	"net/http"

	apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"
	"github.com/gin-gonic/gin"
)

func ErrorHandler() gin.HandlerFunc {
	return func(c *gin.Context) {
		c.Next()

		if len(c.Errors) > 0 {
			err := c.Errors.Last().Err
			var appErr *apperror.AppError

			if errors.As(err, &appErr) {
				c.JSON(appErr.StatusCode, gin.H{
					"message": appErr.Message,
				})

				log.Printf("[ERRO INTERNO] %v\n", appErr.Err)
			}

			log.Printf("[ERRO DESCONHECIDO] %v\n", err)
			c.JSON(http.StatusInternalServerError, gin.H{
				"message": "Erro interno do servidor",
			})
		}
	}
}
