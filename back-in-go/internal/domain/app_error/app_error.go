package apperror

import (
	"fmt"
)

type AppError struct {
	StatusCode int    `json:"-"`
	Message    string `json:"message"`
	Err        error  `json:"-"`
}

func (e *AppError) Error() string {
	if e.Err != nil {
		return fmt.Sprintf("%s: %v", e.Message, e.Err)
	}

	return e.Message
}

func InternalServerError(message string, err error) *AppError {
	return &AppError{
		StatusCode: 500,
		Message:    message,
		Err:        err,
	}
}

func BadRequest(message string, err error) *AppError {
	return &AppError{
		StatusCode: 400,
		Message:    message,
		Err:        err,
	}
}

func Unauthorized(message string, err error) *AppError {
	return &AppError{
		StatusCode: 401,
		Message:    message,
		Err:        err,
	}
}

func NotFound(message string, err error) *AppError {
	return &AppError{
		StatusCode: 404,
		Message:    message,
		Err:        err,
	}
}
