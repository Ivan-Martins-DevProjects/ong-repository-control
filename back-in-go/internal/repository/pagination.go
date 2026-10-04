package repository

import apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"

type Pagination struct {
	Page     int   `json:"page"`
	PageSize int   `json:"page_size"`
	Total    int64 `json:"total"`
	Pages    int   `json:"pages"`
	Offset   int   `json:"offset"`
}

func NewPagination(page, pageSize int, total int64) (*Pagination, error) {
	if page <= 0 || pageSize <= 0 {
		return nil, apperror.BadRequest("Confira se os parâmetros de paginação estão corretos", nil)
	}

	offset := (page - 1) * pageSize
	pages := int((total + int64(pageSize) - 1) / int64(pageSize))

	return &Pagination{
		Page:     page,
		PageSize: pageSize,
		Total:    total,
		Pages:    pages,
		Offset:   offset,
	}, nil
}
