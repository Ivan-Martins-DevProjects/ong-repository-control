import { HttpInterceptorFn } from "@angular/common/http";
import { API_CONTEXT, API_VERSION, RETRY_ENABLED, SKIP_AUTH } from "./base-service";
import { config, throwError } from "rxjs";

interface config {
  name: string
  path: string
}

export const apiInterceptor: HttpInterceptorFn = (req, next) => {

  const config: config[] = [
    {
      name: "product_service",
      path: "/products"
    },
    {
      name: "stock_service",
      path: "/stock"
    }
  ]

  const service = req.context.get(API_CONTEXT)
  const version = req.context.get(API_VERSION)
  const skipAuth = req.context.get(SKIP_AUTH)
  const retry = req.context.get(RETRY_ENABLED)

  let request = req

  const item = config.find(item => item.name === service)
  if (!item) {
    console.error("Service não encontrado");
    return next(req)
  }

  request = req.clone({
    url: `/api/${version}${item?.path}${req.url}`
  })

  if (!skipAuth) {
    const token = localStorage.getItem("access_token")

    request = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    })
  }

  return next(request)
}
