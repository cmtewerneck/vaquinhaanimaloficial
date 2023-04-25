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
        foto: ['']
      });
    }
      
    populateForm(){
      this.editForm.patchValue({
        name: this.user.name,
        email: this.user.email,
        foto: this.user.foto
      });

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


}        