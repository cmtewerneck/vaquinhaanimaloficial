import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { BaseService } from './_bases/base.service';
import { PagedResult } from './_utils/pagedResult';
import { PagarmeCardResponse, PagarmeResponse } from './auth/User';

@Injectable()
export class AppService extends BaseService {

  constructor(private http: HttpClient) { super() }

    // obterTodosPaginado(pageSize: number, pageNumber: number): Observable<PagedResult<Campanha>> {
    //     return this.http
    //         .get<PagedResult<Campanha>>(this.urlServiceV1 + 'campanhas/todos-paginado/' + pageSize + '/' + pageNumber, this.ObterAuthHeaderJson())
    //         .pipe(catchError(super.serviceError));
    // }

    // enviarEmail(contato: Contato): Observable<Contato> {
    //   return this.http
    //       .post(this.urlServiceV1 + 'contatos', contato, this.ObterAuthHeaderJson())
    //       .pipe(
    //           map(super.extractData),
    //           catchError(super.serviceError));
    // }

    // enviarRecorrencia(recorrencia: Recorrencia): Observable<Recorrencia> {
    //   return this.http
    //       .post(this.urlServiceV1 + 'transacoes/add-recorrencia', recorrencia, this.ObterAuthHeaderJson())
    //       .pipe(
    //           map(super.extractData),
    //           catchError(super.serviceError));
    // }

    obterMeusCartoes(): Observable<PagarmeResponse<PagarmeCardResponse>> {
      return this.http
          .get<PagarmeResponse<PagarmeCardResponse>>(this.urlServiceV1 + 'transacoes/list-card', this.ObterAuthHeaderJson())
          .pipe(catchError(super.serviceError));
    }

    obterQuantidadeDoadores(campanhaId: string): Observable<number> {
      return this.http
          .get<number>(this.urlServiceV1 + 'doacoes/total-doadores/' + campanhaId, this.ObterAuthHeaderJson())
          .pipe(catchError(super.serviceError));
  }

}