import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CampanhasRoutingModule } from './campanhas.routing';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { ToastrModule } from 'ngx-toastr';
import { NgxMaskModule } from 'ngx-mask';
import { NgxSpinnerModule } from 'ngx-spinner';
import { CampanhasComponent } from './campanhas.component';
import { ListarTodasComponent } from './listar-todas/listar-todas.component';
import { CampanhaGuard } from './campanha.guard';
import { CriarComponent } from './criar/criar.component';
import { ImageCropperModule } from 'ngx-image-cropper';

@NgModule({
  imports: [
    CommonModule,
    CampanhasRoutingModule,
    RouterModule,
    NgxSpinnerModule,
    ReactiveFormsModule,
    ImageCropperModule,
    NgxMaskModule.forChild(),
    HttpClientModule,
    ToastrModule.forRoot({
      timeOut: 3000,
      positionClass: 'toast-bottom-right',
      preventDuplicates: true,
      maxOpened: 0,
      progressBar: true,
      progressAnimation: 'decreasing'
    }),
    FormsModule
  ],
  declarations: [
    CampanhasComponent,
    ListarTodasComponent,
    CriarComponent
  ],
  providers: [
    CampanhaGuard
  ]
})
export class CampanhasModule { }
