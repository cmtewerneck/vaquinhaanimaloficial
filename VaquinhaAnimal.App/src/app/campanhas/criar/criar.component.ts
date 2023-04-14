import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Dimensions, ImageCroppedEvent, ImageTransform } from 'ngx-image-cropper';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { Campanha } from '../model/Campanha';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Bancos } from '../model/Bancos';

@Component({
  selector: 'app-criar-campanhas',
  templateUrl: './criar.component.html'
})
export class CriarComponent implements OnInit {

  private _jsonURL = '../../../assets/bancos.json';
  document_mask: string = "000.000.000-00";
  document_toggle: string = "CPF";

  get imagensArray(): FormArray {
    return <FormArray>this.campanhaForm.get('imagens');
  }

  // VARIÁVEIS PARA IMAGEM
  imageChangedEvent: any = '';
  croppedImage: any = '';
  canvasRotation = 0;
  rotation = 0;
  scale = 1;
  showCropper = false;
  containWithinAspectRatio = false;
  transform: ImageTransform = {};
  imageUrl!: string;
  imagemNome!: string;
  // FIM DA IMAGEM

  errors: any[] = [];
  campanhaForm!: FormGroup;
  beneficiarioForm!: FormGroup;
  campanha!: Campanha;
  campanhaRecorrente: boolean = false;
  bancos: Bancos[] = [];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    // private campanhaService: CampanhaService,
    private spinner: NgxSpinnerService, 
    private router: Router,
    private toastr: ToastrService) { 
      this.getJSON().subscribe(data => {
        this.bancos = data;
        console.log(data);
       });
    }

    public getJSON(): Observable<any> {
      return this.http.get(this._jsonURL);
    }
    
    ngOnInit(): void {
      this.createForm();
    }

    createForm(){
      this.campanhaForm = this.fb.group({
        titulo: ['', [Validators.required, Validators.maxLength(100), Validators.minLength(3)]],
        tipo_campanha: ['', Validators.required],
        duracao_dias: ['', Validators.required],
        valor_desejado: ['', Validators.required],
        descricao_curta: ['', [Validators.required, Validators.maxLength(200), Validators.minLength(5)]],
        descricao_longa: ['', [Validators.required, Validators.maxLength(5000), Validators.minLength(500)]],
        video_url: [''],
        data_inicio: [''],
        data_encerramento: [''],
        termos: [false, [Validators.required, Validators.requiredTrue]],
        premium: [false, Validators.required],
        imagens: this.fb.array([]),
        beneficiario: this.beneficiarioForm = this.fb.group({
          nome: ['', [Validators.required, Validators.maxLength(100)]],
          documento: [''],
          tipo: ['', Validators.required],
          codigo_banco: ['', [Validators.required, Validators.maxLength(3)]],
          tipo_conta: ['', Validators.required],
          numero_agencia: ['', [Validators.required, Validators.maxLength(4)]],
          digito_agencia: ['', Validators.maxLength(1)],
          numero_conta: ['', [Validators.required, Validators.maxLength(13)]],
          digito_conta: ['', [Validators.required, Validators.maxLength(2)]],
          recebedor_id: ['']
        })
      });
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

    setDocumentValidation(){
      this.beneficiarioForm.controls['documento'].clearValidators();
      this.beneficiarioForm.controls['documento'].setValue("");
      
      if (this.document_toggle == "CPF"){
        this.beneficiarioForm.controls['documento'].setValidators([Validators.required, Validators.minLength(11), Validators.maxLength(11)]);
      } else if(this.document_toggle == "CNPJ"){
        this.beneficiarioForm.controls['documento'].setValidators([Validators.required, Validators.minLength(14), Validators.maxLength(14)]);
      } 
      
      this.beneficiarioForm.controls['documento'].updateValueAndValidity();
      
      console.log(this.beneficiarioForm);
    }

    onCheckboxChange(event: any) {
      if(event.target.checked){
        
      } else {
        
      }
    }

    criaImagem(documento: any): FormGroup {
      return this.fb.group({
        id: [documento.id],
        tipo: [1],
        arquivo: [''],
        arquivo_upload: ['']
      });
    }

    adicionarImagem(){
      this.imagensArray.push(this.criaImagem({ id: "00000000-0000-0000-0000-000000000000" }));
    }

    removerImagem(id: number){
      this.imagensArray.removeAt(id);
    }
    
    adicionarCampanha() {
      this.spinner.show();

      if (this.campanhaForm.dirty && this.campanhaForm.valid) {
        this.campanha = Object.assign({}, this.campanha, this.campanhaForm.value);
        
        this.campanha.imagens.forEach(imagem => {
          imagem.arquivo_upload = this.croppedImage.split(',')[1];
          imagem.arquivo = this.imagemNome;
        });

        // CONVERSÕES
        if (this.campanha.data_inicio) { this.campanha.data_inicio = new Date(this.campanha.data_inicio); } else { this.campanha.data_inicio = null!; }
        if (this.campanha.data_encerramento) { this.campanha.data_encerramento = new Date(this.campanha.data_encerramento); } else { this.campanha.data_encerramento = null!; }
        this.campanha.termos = this.campanha.termos.toString() == "true";
        this.campanha.premium = this.campanha.premium.toString() == "true";

        // this.campanhaService.novaCampanha(this.campanha)
        // .subscribe(
        //   sucesso => { this.processarSucesso(sucesso) },
        //   falha => { this.processarFalha(falha) }
        //   );
        }
      }
      
      processarSucesso(response: any) {
        this.spinner.hide();
        this.campanhaForm.reset();
        this.errors = [];
        
        let toast = this.toastr.success('Campanha cadastrada com sucesso!', 'Sucesso!');
        if (toast) {
          toast.onHidden.subscribe(() => {
            this.router.navigate(['campanhas/minhas-campanhas']);
          });
        }
      }
      
      processarFalha(fail: any) {
        this.spinner.hide();
        this.errors = fail.error.errors;
        this.toastr.error(this.errors[0], "Erro!");
      }

      fileChangeEvent(event: any): void {
        this.imageChangedEvent = event;
        this.imagemNome = event.currentTarget.files[0].name;
        this.adicionarImagem();
      }
      
      imageCropped(event: ImageCroppedEvent) {
        this.croppedImage = event.base64;
      }
      
      imageLoaded() {
        this.showCropper = true;
      }
      
      cropperReady(sourceImageDimensions: Dimensions) {
        console.log('Cropper ready', sourceImageDimensions);
      }
      
      loadImageFailed() {
        this.errors.push('O formato do arquivo ' + this.imagemNome + ' não é aceito.');
      }

      tipoCampanhaSelected(event: any){
        console.log(event.target.value);
        
        if(event.target.value == 1){
          this.campanhaRecorrente = false;
        } else if(event.target.value == 2){
          this.campanhaRecorrente = true;
          this.campanhaForm.get('duracao_dias')?.setValue(null);
        }
      }
}