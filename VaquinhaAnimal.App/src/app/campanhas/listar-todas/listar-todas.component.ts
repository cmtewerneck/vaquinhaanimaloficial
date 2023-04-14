import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-listar-campanhas',
  templateUrl: './listar-todas.component.html'
})
export class ListarTodasComponent implements OnInit {
  
  constructor() {}

  ngOnInit() {
  }

  
}