import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';

interface Result {
  checks: Check[];
  totalStatus: string;
  totalResponseTime: number;
}

interface Check {
  name: string;
  status: string;
  description: string;
  responseTime: number;
}

@Component({
  selector: 'app-health-check',
  templateUrl: './health-check.component.html',
  styleUrls: ['./health-check.component.scss']
})
export class HealthCheckComponent implements OnInit {
  private get url(): string {
    return `${this.baseUrl}hc`;
  }
  public result: Result | undefined;

  constructor(
    private readonly http: HttpClient,
    @Inject('BASE_URL') private readonly baseUrl: string) { }

  public ngOnInit(): void {
    this.http.get<Result>(this.url)
      .subscribe(
        result => this.result = result,
        error => console.error(error));
  }
}
