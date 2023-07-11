import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app-routing.module';
import { NgxMaskModule } from 'ngx-mask';

import { AppComponent } from './app.component';
import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';
import { HomepageComponent } from './homepage/homepage.component';
import { ToastrModule } from 'ngx-toastr';
import { AppService } from './app.service';
import { HttpClientModule } from '@angular/common/http';
import { NgxPaginationModule } from 'ngx-pagination';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LOCALE_ID, DEFAULT_CURRENCY_CODE } from '@angular/core';
import localePt from '@angular/common/locales/pt';
import { CommonModule, registerLocaleData } from '@angular/common';
import { NgxSpinnerModule } from 'ngx-spinner';
import { BlogComponent } from './blog/blog.component';
import { ContatoComponent } from './contato/contato.component';
import { CampanhaService } from './campanhas/campanha.service';
import { TagCampanhaPipe } from './campanhas/tag_campanha.pipe';
import { CampanhasModule } from './campanhas/campanhas.module';
import { AcessoNegadoComponent } from './acesso-negado/acesso-negado.component';
import { TermosComponent } from './termos/termos.component';
registerLocaleData(localePt);

@NgModule({
  declarations: [												
    AppComponent,
    HeaderComponent,
    FooterComponent,
    AcessoNegadoComponent,
    ContatoComponent,
    TermosComponent,
    BlogComponent,
    HomepageComponent
   ],
  imports: [
    CommonModule,
    BrowserModule,
    HttpClientModule,
    NgxSpinnerModule,
    AppRoutingModule,
    CampanhasModule,
    ReactiveFormsModule,
    FormsModule,
    NgxMaskModule.forRoot(),
    ToastrModule.forRoot({
      timeOut: 3000,
      positionClass: 'toast-bottom-right',
      preventDuplicates: true,
      maxOpened: 0,
      progressBar: true,
      progressAnimation: 'decreasing'
    }),
    BrowserAnimationsModule,
    NgxPaginationModule
  ],
  providers: [AppService,
    {
      provide: LOCALE_ID,
      useValue: "pt-BR"
    },
    {
      provide:  DEFAULT_CURRENCY_CODE,
      useValue: 'BRL'
    },],
  bootstrap: [AppComponent]
})
export class AppModule { }
