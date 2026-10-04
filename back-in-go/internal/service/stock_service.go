package service

import (
	"strconv"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain"
	apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/repository"
)

type StockService interface {
	ListAllStockItems(pageQuery, pageSizeQuery string) (*ListAllResponse, error)
}

type stockService struct {
	repo repository.StockRepository
}

func NewStockService(db repository.StockRepository) StockService {
	return &stockService{
		repo: db,
	}
}

type ListAllResponse struct {
	Items []domain.StockItem     `json:"items"`
	Pages *repository.Pagination `json:"pagination"`
}

func (s *stockService) ListAllStockItems(pageQuery, pageSizeQuery string) (*ListAllResponse, error) {
	page, err := strconv.Atoi(pageQuery)
	if err != nil {
		return nil, apperror.BadRequest("Erro ao buscar dados da página solicitada", err)
	}

	pageSize, err := strconv.Atoi(pageSizeQuery)
	if err != nil {
		return nil, apperror.BadRequest("Tamanho limite da página inválido", err)
	}

	total, err := s.repo.CountAllStockItems()
	if err != nil {
		return nil, err
	}

	pg, err := repository.NewPagination(page, pageSize, total)
	if err != nil {
		return nil, err
	}

	stockItems, pagination, err := s.repo.ListAllStockItems(pg)
	if err != nil {
		return nil, err
	}

	return &ListAllResponse{
		Items: stockItems,
		Pages: pagination,
	}, nil
}
