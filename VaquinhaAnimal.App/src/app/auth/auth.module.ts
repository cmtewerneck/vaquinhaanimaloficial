import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthComponent } from './auth.component';
import { AuthRoutingModule } from './auth.routing';
import { RouterModule } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from './auth.service';
import { HttpClientModule } from '@angular/common/http';
import { ToastrModule } from 'ngx-toastr';
import { AddCardComponent } from './wallet/addCard.component';
import { MyWalletComponent } from './wallet/myWallet.component';
import { EditComponent } from './edit/edit.component';
import { NgxMaskModule } from 'ngx-mask';
import { EditPasswordComponent } from './editPassword/editPassword.component';
import { AuthResolve } from './auth.resolve';
import { NgxSpinnerModule } from 'ngx-spinner';
import { ResetPasswordComponent } from './resetPassword/resetPassword.component';
import { ResetPasswordUserComponent } from './resetPasswordUser/resetPasswordUser.component';
import { AuthGuard } from './auth.guard';

@NgModule({
  imports: [
    CommonModule,
    AuthRoutingModule,
    RouterModule,
    NgxSpinnerModule,
    ReactiveFormsModule,
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
    AuthComponent,
    LoginComponent,
    AddCardComponent,
    MyWalletComponent,
    EditComponent,
    ResetPasswordComponent,
    ResetPasswordUserComponent,
    EditPasswordComponent,
    RegisterComponent
  ],
  providers: [
    //AuthService,
    AuthResolve,
    AuthGuard
  ]
})
export class AuthModule { }
