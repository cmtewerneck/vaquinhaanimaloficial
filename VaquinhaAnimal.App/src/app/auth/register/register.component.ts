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
  document_mask: string = "000.000.000-00";
  document_toggle: string = "CPF";
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
        document: [''],
        document_type: ['', Validators.required],
        type: ['', Validators.required],
        gender: ['', Validators.required],
        birthdate: ['', Validators.required],
        line_1: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(150)]],
        line_2: ['', Validators.maxLength(150)],
        zip_code: ['', [Validators.required, Validators.maxLength(8)]],
        city: ['', [Validators.required, Validators.maxLength(50)]],
        state: ['', [Validators.required, Validators.maxLength(50)]],
        country: ['', [Validators.required, Validators.maxLength(50)]],
        foto: ['']
      });
    }
    
    setDocumentValidation(){
      this.registerForm.controls['document'].clearValidators();
      this.registerForm.controls['document'].setValue("");
      
      if (this.document_toggle == "CPF"){
        this.registerForm.controls['document'].setValidators([Validators.required, Validators.minLength(11), Validators.maxLength(11)]);
      } else if(this.document_toggle == "CNPJ"){
        this.registerForm.controls['document'].setValidators([Validators.required, Validators.minLength(14), Validators.maxLength(14)]);
      } else if(this.document_toggle == "PASSPORT"){
        this.registerForm.controls['document'].setValidators([Validators.required, Validators.maxLength(50)]);
      }
      
      this.registerForm.controls['document'].updateValueAndValidity();
      
      console.log(this.registerForm);
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
      
      buscaCep(cep: string){
        console.log("CEP PROCURADO: "+ cep);
        
        this.authService.buscaCep(cep).subscribe(
          success => { 
            this.registerForm.patchValue({
              line_1: success.logradouro,
              city: success.localidade,
              state: success.uf
            });
            
            if(success.erro == "true"){
              this.toastr.error('CEP não encontrado!');
            }
          },
          error => {
            this.toastr.error('CEP inválido!');
          },
          );
          
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
        
        documentSelected(event: any){
          console.log(event.target.value);
          
          if(event.target.value == "CPF"){
            this.document_mask = "000.000.000-00";
            this.document_toggle = "CPF";
          } else if(event.target.value == "CNPJ"){
            this.document_mask = "00.000.000/0000-00"
            this.document_toggle = "CNPJ";
          } else if(event.target.value == "PASSPORT"){
            this.document_mask = ""
            this.document_toggle = "";
          }
          
          this.setDocumentValidation();
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
      