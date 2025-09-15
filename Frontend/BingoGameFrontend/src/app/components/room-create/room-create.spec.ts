import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RoomCreate } from './room-create';

describe('RoomCreate', () => {
  let component: RoomCreate;
  let fixture: ComponentFixture<RoomCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RoomCreate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RoomCreate);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
