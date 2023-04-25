import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { User } from '../User';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html'
})
export class RegisterComponent implements OnInit {
  
  registerForm!: FormGroup;
  errors: any[] = [];
  user!: User;
  passwordInputType: string = "password";
  
  constructor(
    private router: Router, 
    private toastr: ToastrService,
    private authService: AuthService, 
    private spinner: NgxSpinnerService,
    public fb: FormBuilder) { }
    
    ngOnInit() {
      if (localStorage.getItem('token') !== null) {
        this.router.navigate(['/index']);
      }
      this.validation();
    }
    
    validation() {
      this.registerForm = this.fb.group({
        name: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(64)]],
        email: ['', [Validators.required, Validators.minLength(5), Validators.email, Validators.maxLength(64)]],
        password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(12)]],
        confirmPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(12)]],
        foto: ['']
      });
    }
    
    register() {
      this.spinner.show();
      
      if (this.registerForm.dirty && this.registerForm.valid) {
        this.user = Object.assign(this.registerForm.value);
        console.log(this.user);
        
        this.authService.register(this.user).subscribe(
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
        this.toastr.success('Cadastro realizado com sucesso', 'Parabéns!');
        this.errors = [];
        
        this.authService.LocalStorage.salvarDadosLocaisUsuarioSession(response);
        
        this.router.navigate(['/campanhas']);
      }
      
      processarFalha(fail: any) {
        this.spinner.hide();
        this.errors = fail.error.errors;
        
        this.toastr.error(this.errors[0], 'Erro!');
      }
      
      onCheckboxChange(event: any) {
        if(event.target.checked){
          this.passwordInputType = "text";
        } else this.passwordInputType = "password";
      }
      
      resetForm(){
        this.registerForm.reset();
      }
      
}