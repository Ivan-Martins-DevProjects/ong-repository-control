import { HttpClient, HttpContext, HttpContextToken } from "@angular/common/http";
import { inject } from "@angular/core/primitives/di";

export enum ApiVersion {
  V1 = 'v1',
  V2 = 'v2',
  V3 = 'v3'
}

export const API_VERSION = new HttpContextToken<ApiVersion>(
  () => ApiVersion.V1
)

export const API_CONTEXT = new HttpContextToken<string>(
  () => 'default'
)

export const SKIP_AUTH = new HttpContextToken<boolean>(
  () => false
)

export const RETRY_ENABLED = new HttpContextToken<boolean>(
  () => true
)

export abstract class BasicService {
  protected readonly http = inject(HttpClient)

  protected get<T>(url: string, version: ApiVersion) {
    const context = new HttpContext()
      .set(API_VERSION, version)

    return this.http.get<T>(url, {
      context
    })
  }


}
