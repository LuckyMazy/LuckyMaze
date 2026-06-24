import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient, HttpResponse, HttpEvent, HttpContext } from '@angular/common/http';
import { Observable } from 'rxjs';

import { BASE_PATH } from '../variables';
import { Configuration } from '../configuration';
import { BaseService } from '../api.base.service';

export enum MazeSize {
  Small16x16 = 16,
  Medium32x32 = 32,
  Large64x64 = 64
}

export interface GameSettings {
  id?: string;
  createdAt?: string;
  updatedAt?: string;
  mazeSize?: MazeSize;
  gameSpeedMs?: number;
  minBet?: number;
  maxBet?: number;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService extends BaseService {

    constructor(protected httpClient: HttpClient, @Optional() @Inject(BASE_PATH) basePath: string|string[], @Optional() configuration?: Configuration) {
        super(basePath, configuration);
    }

    public apiSettingsGet(observe: any = 'body', reportProgress: boolean = false, options?: {context?: HttpContext, transferCache?: boolean}): Observable<any> {
        let localVarHeaders = this.defaultHeaders;
        localVarHeaders = this.configuration.addCredentialToHeaders('OAuth2', 'Authorization', localVarHeaders, 'Bearer ');
        const localVarHttpContext: HttpContext = options?.context ?? new HttpContext();
        let localVarPath = `/api/Settings`;
        const { basePath, withCredentials } = this.configuration;
        return this.httpClient.request<GameSettings>('get', `${basePath}${localVarPath}`, {
            context: localVarHttpContext,
            responseType: 'json',
            ...(withCredentials ? { withCredentials } : {}),
            headers: localVarHeaders,
            observe: observe,
            reportProgress: reportProgress
        });
    }

    public apiSettingsPut(settings: GameSettings, observe: any = 'body', reportProgress: boolean = false, options?: {context?: HttpContext, transferCache?: boolean}): Observable<any> {
        let localVarHeaders = this.defaultHeaders;
        localVarHeaders = this.configuration.addCredentialToHeaders('OAuth2', 'Authorization', localVarHeaders, 'Bearer ');
        const localVarHttpContext: HttpContext = options?.context ?? new HttpContext();
        let localVarPath = `/api/Settings`;
        const { basePath, withCredentials } = this.configuration;
        return this.httpClient.request<any>('put', `${basePath}${localVarPath}`, {
            body: settings,
            context: localVarHttpContext,
            responseType: 'json',
            ...(withCredentials ? { withCredentials } : {}),
            headers: localVarHeaders,
            observe: observe,
            reportProgress: reportProgress
        });
    }
}
