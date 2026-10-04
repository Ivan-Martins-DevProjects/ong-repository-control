package repository

import (
	"gorm.io/gorm"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain"
	apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"
)

type StockRepository interface {
	ListAllStockItems(pg *Pagination) ([]domain.StockItem, *Pagination, error)
	CountAllStockItems() (int64, error)
}

type stockRepository struct {
	db *gorm.DB
}

func NewStockRepository(db *gorm.DB) StockRepository {
	return &stockRepository{db: db}
}

type StockItems struct {
	Items []domain.StockItem
}

func (s *stockRepository) ListAllStockItems(pg *Pagination) ([]domain.StockItem, *Pagination, error) {
	var items []domain.StockItem

	if err := s.db.Order("id ASC").Limit(pg.PageSize).Offset(pg.Offset).Find(&items).Error; err != nil {
		return nil, pg, err
	}

	return items, pg, nil
}

func (s *stockRepository) CountAllStockItems() (int64, error) {
	var total int64
	if err := s.db.Model(&domain.StockItem{}).Count(&total).Error; err != nil {
		return 0, apperror.InternalServerError("Erro ao contabilizar todos os itens do estoque", err)
	}

	return total, nil
}
