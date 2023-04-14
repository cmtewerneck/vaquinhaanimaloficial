import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../auth.service';
import { BuscaCep } from '../BuscaCep';
import { User } from '../User';
import * as moment from 'moment';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html'
})
export class EditComponent implements OnInit {
  
  editForm!: FormGroup;
  errors: any[] = [];
  user!: User;
  enderecoRecebido!: BuscaCep;
  document_mask: string = "000.000.000-00";
  document_toggle: string = "CPF";
  genderReturned!: string;
  docTypeReturned!: string;
  docNumberReturned!: string;
  
  constructor(
    private router: Router, 
    private toastr: ToastrService,
    private authService: AuthService, 
    private spinner: NgxSpinnerService,
    public fb: FormBuilder, 
    private route: ActivatedRoute) { this.user = this.route.snapshot.data['user']; }
    
    ngOnInit() {
      this.createForm();
      this.populateForm();
    }
    
    createForm() {
      this.spinner.show();
      
      this.editForm = this.fb.group({
        name: ['', [Validators.required, Validators.maxLength(64)]],
        email: ['', [Validators.required, Validators.email, Validators.maxLength(64)]],
        document: [''],
        document_type: ['', [Validators.required, Validators.maxLength(20)]],
        gender: ['', [Validators.required, Validators.maxLength(20)]],
        birthdate: ['', Validators.required],
        line_1: ['', [Validators.required, Validators.maxLength(150)]],
        line_2: ['', Validators.maxLength(150)],
        zip_code: ['', [Validators.required, Validators.maxLength(20)]],
        city: ['', [Validators.required, Validators.maxLength(50)]],
        state: ['', [Validators.required, Validators.maxLength(50)]],
        country: ['', [Validators.required, Validators.maxLength(50)]],
        foto: ['']
      });
    }
      
    populateForm(){
      this.editForm.patchValue({
        name: this.user.name,
        email: this.user.email,
        document: this.user.document,
        document_type: this.user.document_type,
        gender: this.user.gender,
        birthdate: moment(this.user.birthdate).format("YYYY-MM-DD"),
        line_1: this.user.line_1,
        line_2: this.user.line_2,
        zip_code: this.user.zip_code,
        city: this.user.city,
        state: this.user.state,
        country: this.user.country,
        foto: this.user.foto
      });

      this.genderReturned = this.user.gender;
      this.docTypeReturned = this.user.document_type;
      this.docNumberReturned = this.user.document;

      this.spinner.hide();
    }
      
    edit() {
      this.spinner.show();

      if (this.editForm.dirty && this.editForm.valid) {
        this.user = Object.assign(this.editForm.value);
        console.log(this.user);
        
        this.authService.editUser(this.user).subscribe(
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
      this.editForm.reset();
      this.errors = [];
      
      let toast = this.toastr.success('Dados atualizados com sucesso!', 'Sucesso!');
      if (toast) {
        toast.onHidden.subscribe(() => {
          this.router.navigate(['homepage']);
        });
      }
    }   

    processarFalha(fail: any) {
      this.spinner.hide();
      this.errors = fail.error.errors;

      this.toastr.error(this.errors[0], 'Erro!');
    }

    buscaCep(cep: string){
      console.log("CEP PROCURADO: "+ cep);
      
      this.authService.buscaCep(cep).subscribe(
        success => { 
          this.editForm.patchValue({
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

      setDocumentValidation(){
        this.editForm.controls['document'].clearValidators();
        this.editForm.controls['document'].setValue("");

        if (this.document_toggle == "CPF"){
          this.editForm.controls['document'].setValidators([Validators.required, Validators.minLength(11), Validators.maxLength(11)]);
        } else if(this.document_toggle == "CNPJ"){
          this.editForm.controls['document'].setValidators([Validators.required, Validators.minLength(14), Validators.maxLength(14)]);
        }

        this.editForm.controls['document'].updateValueAndValidity();

        console.log(this.editForm);
      }

      documentSelected(event: any){
        console.log(event.target.value);
        
        if(event.target.value == "CPF"){
          this.document_mask = "000.000.000-00";
          this.document_toggle = "CPF";
        } else if(event.target.value == "CNPJ"){
          this.document_mask = "00.000.000/0000-00"
          this.document_toggle = "CNPJ";
        }

        this.setDocumentValidation();
      }

}        