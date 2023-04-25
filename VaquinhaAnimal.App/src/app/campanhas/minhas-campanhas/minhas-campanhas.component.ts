import { Component, OnInit } from '@angular/core';
import { Campanha } from '../model/Campanha';
import { CampanhaService } from '../campanha.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-minhas-campanhas',
  templateUrl: './minhas-campanhas.component.html'
})
export class MinhasCampanhasComponent implements OnInit {

  campanhas!: Campanha[];
  campanha!: Campanha;
  errors!: any[];
  campanhaId!: string;
  
  constructor(private campanhaService: CampanhaService, private spinner: NgxSpinnerService, private toastr: ToastrService) {}

  ngOnInit() {
    this.spinner.show();

    this.ObterTodos();
  }

  ObterTodos() {
    this.campanhaService.obterMinhasCampanhas().subscribe(
      (_campanhas: Campanha[]) => {
      this.campanhas = _campanhas;

      this.spinner.hide();

    }, error => {
        this.spinner.hide();
        this.toastr.error(`Erro de carregamento: ${error.error.errors}`);
        console.log(error);
    });
  }

  salvarId(id: string){
    this.campanhaId = id;
  }

  limparId(){
    this.campanhaId = '';
  }

  enviarParaAnalise() {
    this.spinner.show();

    this.campanhaService.enviarParaAnalise(this.campanhaId)
    .subscribe(
      campanha => { 
        this.processarSucesso(campanha) 
      },
      falha => { this.processarFalha(falha) }
      )
  }

  deletarCampanha() {
    this.spinner.show();

    this.campanhaService.excluirCampanha(this.campanhaId)
    .subscribe(
      campanha => { 
        this.processarSucessoExclusao(campanha) 
      },
      falha => { this.processarFalha(falha) }
      )
  }

  processarSucesso(response: any) {
    this.spinner.hide();
    this.errors = [];
    this.toastr.success('Campanha enviada para análise!', 'Sucesso!');
    this.ObterTodos();
    this.campanhaId = "";
  }

  processarSucessoExclusao(response: any) {
    this.spinner.hide();
    this.errors = [];
    this.toastr.success('Campanha excluída com sucesso!', 'Sucesso!');
    this.ObterTodos();
    this.campanhaId = "";
  }
  
  processarFalha(fail: any) {
    this.spinner.hide();
    this.errors = fail.error.errors;
    this.toastr.error('Ocorreu um erro!', 'Opa :(');
    this.campanhaId = "";
  }
  
}