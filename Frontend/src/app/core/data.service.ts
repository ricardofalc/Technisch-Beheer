// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Cache } from './cache';
import { AuthorizedHttpService } from './authorized-http.service';
import { SpinnerService } from './spinner.service';

import { EnvironmentSettings, EnvironmentSettingsService } from './environment-settings.service';
import 'rxjs/add/operator/map';

@Injectable()
export class DataService {

    private cacheMap: Map<string, Cache<any>>;

    constructor(
        private environmentSettingsService: EnvironmentSettingsService,
        private authHttpService: AuthorizedHttpService,
        private spinnerService: SpinnerService) {
        this.cacheMap = new Map<string, Cache<any>>();
    }

    get<T>(path: string): Observable<T[]> {
        this.spinnerService.start();
        const cache = this.getCache<T>(path);
        const url = this.getUrl(path);

        this.authHttpService.get(url)
            .map(response => response.json() as T[])
            .subscribe(data => {
                cache.set(data);
                this.spinnerService.stop();
            },
            error => this.spinnerService.stop());

        return cache.getItems();
    }

    getNoCache<T>(path: string): Observable<T[]> {
        const url = this.getUrl(path);

        const observable = this.authHttpService.get(url)
            .map(response => response.json() as T[]);

        return observable;
    }

    getSingleNoCache<T>(path: string): Observable<T> {
        const url = this.getUrl(path);

        const observable = this.authHttpService.get(url)
            .map(response => response.json() as T);

        return observable;
    }

    getSingle<T>(path: string, id?: string | number): Observable<T> {
        const cache = this.getCache(path);
        const url = this.getUrl(path, id);
        const subject = new BehaviorSubject<T>(cache.get(id) as T);

        this.authHttpService.get(url)
            .map(response => response.json() as T)
            .subscribe(data => { cache.update(data); subject.next(data); }, error => subject.error(error));

        return subject.asObservable();
    }

    post<T>(path: string, data: T, queryParameters: Map<string, string> = null): Observable<any> {
        const cache = this.getCache(path);
        let url = this.getUrl(path);

        if (queryParameters != null) {
            queryParameters.forEach((value: string, key: string) => {
                if (key === queryParameters.keys().next().value) {
                    url = url.concat('?' + key + '=' + value);
                } else {
                    url = url.concat('&' + key + '=' + value);
                }
            });
        }

        const observable = this.authHttpService.post(url, data)
            .map(response => response.text() ? response.json() : null);

        observable
            .subscribe(d => cache.add(d), error => { });

        return observable;
    }

    put<T>(path: string, id: string | number, data: T, updateAll: boolean): Observable<any> {
        const cache = this.getCache(path);
        const url = this.getUrl(path, id);
        const observable = this.authHttpService.put(url, data).map(response => response.text() ? response.json() : null);

        observable
            .subscribe(d => {
                if (updateAll) {
                    this.get<T>(path);
                } else {
                    cache.update(d);
                }
            });

        return observable;
    }

    delete<T>(path: string, id: string | number): Observable<any> {
        const cache = this.getCache(path);
        const url = this.getUrl(path, id);
        const observable = this.authHttpService.delete(url);

        observable
            .subscribe(d => this.get<T>(path));

        return observable;
    }

    private getUrl(path: string, id?: string | number) {
        const url = `${this.environmentSettingsService.getEnvironmentVariable(EnvironmentSettings.backendUrl)}/${path}`;
        return id ? `${url}/${id}` : url;
    }

    private getCache<T>(path): Cache<T> {
        if (!this.cacheMap.has(path)) {
            this.cacheMap.set(path, new Cache());
        }

        return this.cacheMap.get(path);
    }
}
