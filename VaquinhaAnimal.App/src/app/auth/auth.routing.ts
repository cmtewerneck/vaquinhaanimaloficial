import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthComponent } from './auth.component';
import { AuthResolve } from './auth.resolve';
import { EditComponent } from './edit/edit.component';
import { EditPasswordComponent } from './editPassword/editPassword.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ResetPasswordComponent } from './resetPassword/resetPassword.component';
import { ResetPasswordUserComponent } from './resetPasswordUser/resetPasswordUser.component';
import { AddCardComponent } from './wallet/addCard.component';
import { MyWalletComponent } from './wallet/myWallet.component';

const authRouterConfig: Routes = [
    {
        path: '', component: AuthComponent,
        children: [
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent },
            { path: 'edit-user/:id', component: EditComponent, resolve: { user: AuthResolve }},
            { path: 'edit-password', component: EditPasswordComponent },
            { path: 'reset-password', component: ResetPasswordComponent },
            { path: 'reset-password-user/:username/:token', component: ResetPasswordUserComponent },
            { path: 'add-card', component: AddCardComponent },
            { path: 'wallet', component: MyWalletComponent }
        ]
    }
];

@NgModule({
    imports: [
        RouterModule.forChild(authRouterConfig)
    ],
    exports: [RouterModule]
})
export class AuthRoutingModule { }