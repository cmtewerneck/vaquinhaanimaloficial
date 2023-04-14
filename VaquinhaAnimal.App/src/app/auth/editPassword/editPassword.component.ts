import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../auth.service';
import { UserPassword } from '../UserPassword';

@Component({
  selector: 'app-edit-password',
  templateUrl: './editPassword.component.html'
})
export class EditPasswordComponent implements OnInit {
  
  editPasswordForm!: FormGroup;
  errors: any[] = [];
  user!: UserPassword;
  mostrarSenhas: string = "password";
  
  constructor(
    private router: Router, 
    private toastr: ToastrService,
    private authService: AuthService, 
    private spinner: NgxSpinnerService,
    public fb: FormBuilder, 
    private route: ActivatedRoute) { }
    
    ngOnInit() {
      this.editPasswordForm = this.fb.group({
        currentPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
        newPassword: ['', [Validators.required, Validators.minLength(6),Validators.maxLength(100)]],
        confirmPassword: ['', [Validators.required, Validators.minLength(6),Validators.maxLength(100)]]
      });
    }
    
    mostrarSenha(){
      if (this.mostrarSenhas == "text"){
        this.mostrarSenhas = "password"
      } else if(this.mostrarSenhas == "password"){
        this.mostrarSenhas = "text"
      }
    }
    
    editPassword() {
      this.spinner.show();

      if (this.editPasswordForm.dirty && this.editPasswordForm.valid) {
        this.user = Object.assign(this.editPasswordForm.value);
        console.log(this.user);
        
        this.authService.editUserPassword(this.user).subscribe(
          success => { 
            this.processarSucesso(success); 
          },
          error => {
            this.processarFalha(error);
          },
          );
          
        }
      }
      
      processarSucesso(response: any) {
        this.spinner.hide();
        this.editPasswordForm.reset();
        this.errors = [];
        
        let toast = this.toastr.success('Senha atualizada com sucesso!', 'Sucesso!');
        if (toast) {
          toast.onHidden.subscribe(() => {
            this.router.navigate(['homepage']);
          });
        }
      }
      
      processarFalha(fail: any) {
        this.spinner.hide();
        this.errors = fail.error.errors;
        this.toastr.error(this.errors[0]);
      }
      
    }
    