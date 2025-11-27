# 📐 Architecture & Flow Diagrams

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    User's Browser                           │
│                   (Customer Interface)                      │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┴─────────────┐
        │                          │
        ▼                          ▼
    Shopping Page            Cart Page
    - Browse products        - View items
    - Add to cart            - Checkout button
        │                         │
        │                         └────┬─────────────────┐
        │                             │                 │
        │                          [Click Checkout]     │
        │                             │                 │
        │                             ▼                 │
        │                    ┌─────────────────┐        │
        │                    │ Check if logged │        │
        │                    │ in (UserId)?    │        │
        │                    └────────┬────────┘        │
        │                             │                 │
        │          ┌──────────────────┴──────────────────┐
        │          │                                     │
        ▼          ▼                                     ▼
    [Login Required]                              [Create Order]
    Redirect to:                                  - Calculate total
    /User/Login                                   - Save to database
    (YOUR login page)                             - Clear cart
        │
        └─────────────────────────┬──────────────────────┐
                                  │                      │
                            [Login Success]             │
                                  │                      │
                                  └────────┬─────────────┘
                                           │
                                           ▼
                                    ┌──────────────┐
                                    │ Both paths   │
                                    │ success →    │
                                    │ Shop page    │
                                    └──────────────┘
```

---

## Checkout Flow Sequence

```
UNAUTHENTICATED USER                          AUTHENTICATED USER
─────────────────────                        ──────────────────

1. Browse Shop                                1. Browse Shop
   └─ Add to cart                               └─ Add to cart

2. Click Checkout                             2. Click Checkout
   │                                            │
3. CartController.Checkout()                  3. CartController.Checkout()
   │                                            │
4. Check: User.Identity.IsAuthenticated       4. Check: User.Identity.IsAuthenticated
   │                                            │
5. FALSE ──────────┐                          5. TRUE
   │               │                             │
6. Redirect        │                          6. Get cart from session
   to /User/Login  │                             │
   │               │                          7. Calculate total
7. See login form  │                             │
   (YOUR page)     │                          8. Create Order object
   │               │                             │
8. Enter email     │                          9. Save to database
   & password      │                             │
   │               │                          10. Clear cart
9. Click Sign In   │                             │
   │               │                          11. Show success message
10. Login succeeds │                             │
    UserManager    │                          12. Redirect to Shop
    authenticates  │                             
    │              │
11. Manual return  │
    to shop/cart   │
    │              │
12. Add to cart    │
    │              │
13. Click Checkout│
    │              │
14. CartController└──→ (SAME AS STEPS 3-12 FOR AUTHENTICATED)
    .Checkout()
    │
    (Follow authenticated flow above)
```

---

## Payment History Flow

```
┌─────────────────────────────────┐
│  Logged-in User on Any Page     │
│                                 │
│  [Top-right Navbar]             │
│  Shop | Cart | [📋 History]     │
│                └─────┬──────────┘
│                      │
│              User clicks icon
│                      │
│                      ▼
│           ┌──────────────────┐
│           │ Modal opens      │
│           │ Shows spinner    │
│           │ "Loading..."     │
│           └────────┬─────────┘
│                    │
│    Browser sends: GET /Cart/PaymentHistory
│                    │
│                    ▼
│    ┌──────────────────────────────┐
│    │ CartController               │
│    │ .PaymentHistory()            │
│    │                              │
│    │ 1. Check if authenticated    │
│    │    (User != null)            │
│    │ 2. Query database:           │
│    │    SELECT * FROM Orders      │
│    │    WHERE UserId = current_id │
│    │ 3. Load product details      │
│    │ 4. Return partial view       │
│    └────────────┬─────────────────┘
│                 │
│    Server returns: HTML of order table
│                 │
│                 ▼
│    ┌──────────────────────────────┐
│    │ JavaScript replaces modal    │
│    │ content with HTML            │
│    └────────────┬─────────────────┘
│                 │
│                 ▼
│    ┌──────────────────────────────┐
│    │ User sees orders table:      │
│    │ Date | Items | Total | Status│
│    │ Nov 27 | 2  | $599 | Pending│
│    │ Nov 26 | 1  | $899 | Pending│
│    │ Nov 25 | 3  | $299 | Pending│
│    └────────────┬─────────────────┘
│                 │
│         User clicks order
│                 │
│                 ▼
│    ┌──────────────────────────────┐
│    │ JavaScript toggles           │
│    │ visibility of details row    │
│    │                              │
│    │ Shows:                       │
│    │ - iPhone 15 Pro × 1 = $999   │
│    │ - Case × 2 = $49             │
│    │ - Cable × 1 = $15            │
│    └──────────────────────────────┘
│
└─────────────────────────────────┘
```

---

## Database Flow

```
┌─────────────────────────────┐
│ CartController.Checkout()   │
│                             │
│ Calculate total locally     │
│ (server-side safe)          │
└────────────┬────────────────┘
             │
             ▼
      ┌────────────────┐
      │ Create Order   │
      │ object:        │
      │                │
      │ Id: 1          │
      │ UserId: user1  │
      │ Status: Pend   │
      │ CreatedAt: NOW │
      │ Total: $599    │
      │ Items: [...]   │
      └────────┬───────┘
               │
               ▼
      ┌──────────────────────┐
      │ Save to Database:    │
      │                      │
      │ INSERT INTO Orders   │
      │ (UserId, Status,     │
      │  CreatedAt, Total)   │
      │                      │
      │ INSERT INTO          │
      │ OrderProducts        │
      │ (OrderId, ProductId, │
      │  Quantity)           │
      └────────┬─────────────┘
               │
               ▼
      ┌──────────────────────┐
      │ Orders Table:        │
      │                      │
      │ Id   UserId    Total │
      │ 1    user1@... 599   │
      │ 2    user1@... 899   │
      │ 3    user2@... 299   │
      │                      │
      │ OrderProducts:       │
      │                      │
      │ OrderId ProductId    │
      │ 1       5           │
      │ 1       8           │
      │ 2       3           │
      │ 3       7           │
      └──────────────────────┘
```

---

## User Isolation (Security)

```
┌─────────────────────────────┐
│ Payment History Query       │
│                             │
│ var orders = _context       │
│   .Orders                   │
│   .Where(o =>               │
│     o.UserId ==             │
│     currentUser.Id          │
│   )                         │
│   .ToList();                │
└────────────┬────────────────┘
             │
        [This query...]
        [...only returns orders]
        [...for the current user]
             │
             ▼
    ┌─────────────────┐
    │ All Orders in DB│
    │                 │
    │ UserId: user1   │
    │ UserId: user2   │
    │ UserId: user3   │
    │ UserId: user1   │
    │ UserId: user2   │
    └────────┬────────┘
             │
      Filter WHERE 
      UserId = user1
             │
             ▼
    ┌─────────────────┐
    │ Return ONLY     │
    │ user1's orders  │
    │                 │
    │ UserId: user1 ✓ │
    │ UserId: user1 ✓ │
    │                 │
    │ (user2, user3   │
    │  NOT included)  │
    └─────────────────┘

User2 accessing payment history
would see ONLY their orders!
```

---

## Component Interaction

```
                    ┌──────────────────┐
                    │  _Layout.cshtml  │
                    │                  │
                    │ [Payment History]│
                    │    Icon Modal    │
                    └────────┬─────────┘
                             │
                 ┌───────────┴──────────┐
                 │                      │
                 ▼                      ▼
          JavaScript AJAX      _PaymentHistoryPartial.cshtml
          GET /Cart/            - Table layout
          PaymentHistory        - Status badges
                 │               - Expandable rows
                 │               - Toggle JS
                 │
                 ├────→ CartController
                 │      .PaymentHistory()
                 │           │
                 │           ├─→ Verify user
                 │           ├─→ Query database
                 │           └─→ Return partial view
                 │
                 └────→ Update modal with HTML
                        Show to user
```

---

## Order Lifecycle

```
Step 1: User clicks Checkout
   │
   └─→ Verify authenticated
       └─→ Not authenticated: Redirect to login
       └─→ Authenticated: Continue

Step 2: Create order object
   │
   └─→ UserId (from auth)
   └─→ Status = "Pending" (new)
   └─→ CreatedAt = Now
   └─→ Total = Calculated
   └─→ OrderProducts = Cart items

Step 3: Save to database
   │
   └─→ INSERT Orders record
   └─→ INSERT OrderProducts records
   └─→ Transaction: All or nothing

Step 4: Post-checkout
   │
   └─→ Clear cart from session
   └─→ Show success message
   └─→ Redirect to shop

Step 5: Future - Payment History
   │
   └─→ Query orders by UserId
   └─→ Load product details
   └─→ Display in modal
   └─→ User can expand/collapse
```

---

## HTTP Request/Response Flow

### Checkout POST Request
```
POST /Cart/Checkout HTTP/1.1
Host: localhost:5001
Content-Type: application/x-www-form-urlencoded
Cookie: .AspNetCore.Identity.Application=...

(empty body - just the form submission)

Response:
HTTP/1.1 302 Found
Location: /Shop
Set-Cookie: (session updated)

(User redirected to shop)
```

### Payment History GET Request
```
GET /Cart/PaymentHistory HTTP/1.1
Host: localhost:5001
Accept: text/html
Cookie: .AspNetCore.Identity.Application=...

Response:
HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8

<div class="payment-history-container">
  <table class="table table-hover">
    <thead>
      <tr>
        <th>Order Date</th>
        <th>Items</th>
        <th>Total</th>
        <th>Status</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>Nov 27, 2025</td>
        <td>2 items</td>
        <td>$599.99</td>
        <td><span class="badge badge-warning">Pending</span></td>
      </tr>
      ... more rows ...
    </tbody>
  </table>
</div>

(HTML inserted into modal)
```

---

## File Dependencies

```
CartController.cs
├── Uses: UserManager (from Identity)
├── Uses: AppDBContext (database)
├── Uses: Order model
├── Uses: OrderProduct model
├── Uses: Product model
└── Returns: Views/Cart/_PaymentHistoryPartial.cshtml

_Layout.cshtml
├── Links to: _PaymentHistoryPartial (via AJAX)
├── Links to: Bootstrap modal
└── Includes: JavaScript for AJAX loading

_PaymentHistoryPartial.cshtml
├── Receives: List<Order> model
├── Shows: Orders table
├── Shows: Expandable details
└── Includes: JavaScript for toggle
```

---

## Success Criteria Met

✅ Check if customer is logged in
```
User.Identity.IsAuthenticated
UserManager.GetUserAsync(User)
```

✅ Redirect to existing login if not logged in
```
RedirectToAction("Login", "User")
```

✅ Allow checkout when logged in
```
Create order and save to database
```

✅ Store in database
```
_context.Orders.Add(order)
_context.SaveChangesAsync()
```

✅ Payment history icon in top-right
```
Icon in navbar (visible when authenticated)
```

✅ Click to show payment history
```
Modal opens with list of orders
```

✅ View purchased history
```
Orders displayed with items, dates, totals, status
Expandable rows showing item details
```

---

**All requirements met! ✅ Ready to deploy!**
