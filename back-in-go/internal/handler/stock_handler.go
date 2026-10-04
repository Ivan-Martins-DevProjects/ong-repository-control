package handler

import (
	"net/http"

	"github.com/gin-gonic/gin"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/service"
)

type StockHandler struct {
	stockService service.StockService
}

func NewStockHandler(stockService service.StockService) *StockHandler {
	return &StockHandler{
		stockService: stockService,
	}
}

func (s *StockHandler) GetAllItems(c *gin.Context) {
	pageQuery := c.Query("page")
	pageSizeQuery := c.Query("page_size")

	response, err := s.stockService.ListAllStockItems(pageQuery, pageSizeQuery)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusAccepted, response)
}
